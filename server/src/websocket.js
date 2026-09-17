/**
 * NauraLauncher Real-time RFC 6455 WebSocket Server
 * High performance, zero external dependencies, native TLS/WSS compatible.
 * Supports subscriptions, heartbeat ping/pong, real-time bid broadcasts.
 */

const crypto = require('crypto');
const { EventEmitter } = require('events');

const WS_MAGIC = '258EAFA5-E914-47DA-95CA-C5AB0DC85B11';

class WebSocketConnection extends EventEmitter {
    constructor(socket, isSecure = false) {
        super();
        this.socket = socket;
        this.isSecure = isSecure;
        this.id = crypto.randomUUID();
        this.isAlive = true;
        this.user = null;
        this.subscriptions = new Set(['global']);
        this.buffer = Buffer.alloc(0);

        this.socket.on('data', chunk => this._onData(chunk));
        this.socket.on('close', () => this._onClose());
        this.socket.on('error', err => {
            this.emit('error', err);
            this.close(1011, 'Socket error');
        });
    }

    _onData(chunk) {
        this.buffer = Buffer.concat([this.buffer, chunk]);
        this._parseFrames();
    }

    _parseFrames() {
        while (this.buffer.length >= 2) {
            const byte1 = this.buffer[0];
            const byte2 = this.buffer[1];

            const fin = (byte1 & 0x80) === 0x80;
            const opcode = byte1 & 0x0f;
            const isMasked = (byte2 & 0x80) === 0x80;
            let payloadLen = byte2 & 0x7f;

            let headerOffset = 2;
            if (payloadLen === 126) {
                if (this.buffer.length < 4) return;
                payloadLen = this.buffer.readUInt16BE(2);
                headerOffset = 4;
            } else if (payloadLen === 127) {
                if (this.buffer.length < 10) return;
                // Read 64-bit length (safe integer check)
                const high = this.buffer.readUInt32BE(2);
                const low = this.buffer.readUInt32BE(6);
                payloadLen = high * 2 ** 32 + low;
                headerOffset = 10;
            }

            let maskKey = null;
            if (isMasked) {
                if (this.buffer.length < headerOffset + 4) return;
                maskKey = this.buffer.subarray(headerOffset, headerOffset + 4);
                headerOffset += 4;
            }

            if (this.buffer.length < headerOffset + payloadLen) {
                // Wait for remainder of payload
                return;
            }

            const rawPayload = this.buffer.subarray(headerOffset, headerOffset + payloadLen);
            this.buffer = this.buffer.subarray(headerOffset + payloadLen);

            let payload = Buffer.from(rawPayload);
            if (isMasked && maskKey) {
                for (let i = 0; i < payload.length; i++) {
                    payload[i] ^= maskKey[i % 4];
                }
            }

            this._handleFrame(opcode, payload);
        }
    }

    _handleFrame(opcode, payload) {
        switch (opcode) {
            case 0x1: // Text frame
                const text = payload.toString('utf8');
                this.emit('message', text);
                break;
            case 0x2: // Binary frame
                this.emit('binary', payload);
                break;
            case 0x8: // Close frame
                let code = 1000;
                let reason = '';
                if (payload.length >= 2) {
                    code = payload.readUInt16BE(0);
                    reason = payload.subarray(2).toString('utf8');
                }
                this.close(code, reason);
                break;
            case 0x9: // Ping
                this._sendFrame(0xA, payload); // Respond with Pong
                this.isAlive = true;
                break;
            case 0xA: // Pong
                this.isAlive = true;
                this.emit('pong', payload);
                break;
            default:
                break;
        }
    }

    _sendFrame(opcode, payloadBuffer) {
        if (this.socket.destroyed || !this.socket.writable) return;

        const payloadLen = payloadBuffer.length;
        let header;

        if (payloadLen <= 125) {
            header = Buffer.alloc(2);
            header[0] = 0x80 | (opcode & 0x0f); // FIN + opcode
            header[1] = payloadLen; // Server to client frames must NOT be masked
        } else if (payloadLen <= 65535) {
            header = Buffer.alloc(4);
            header[0] = 0x80 | (opcode & 0x0f);
            header[1] = 126;
            header.writeUInt16BE(payloadLen, 2);
        } else {
            header = Buffer.alloc(10);
            header[0] = 0x80 | (opcode & 0x0f);
            header[1] = 127;
            header.writeBigUInt64BE(BigInt(payloadLen), 2);
        }

        try {
            this.socket.write(Buffer.concat([header, payloadBuffer]));
        } catch {
            this.close(1006, 'Write error');
        }
    }

    send(data) {
        if (typeof data === 'object') {
            data = JSON.stringify(data);
        }
        const buf = Buffer.from(data, 'utf8');
        this._sendFrame(0x1, buf);
    }

    ping(payload = 'ping') {
        this._sendFrame(0x9, Buffer.from(payload));
    }

    close(code = 1000, reason = '') {
        if (this.socket.destroyed) return;
        const reasonBuf = Buffer.from(reason, 'utf8');
        const payload = Buffer.alloc(2 + reasonBuf.length);
        payload.writeUInt16BE(code, 0);
        reasonBuf.copy(payload, 2);
        this._sendFrame(0x8, payload);
        this.socket.end();
    }

    _onClose() {
        this.emit('close');
    }
}

class WebSocketServer extends EventEmitter {
    constructor() {
        super();
        this.clients = new Set();

        // Heartbeat interval to check dead clients
        this.heartbeatTimer = setInterval(() => {
            for (const client of this.clients) {
                if (!client.isAlive) {
                    client.close(1006, 'Heartbeat timeout');
                    this.clients.delete(client);
                    continue;
                }
                client.isAlive = false;
                client.ping();
            }
        }, 30000);
    }

    handleUpgrade(req, socket, head, isSecure = false) {
        const key = req.headers['sec-websocket-key'];
        if (!key) {
            socket.write('HTTP/1.1 400 Bad Request\r\n\r\n');
            socket.destroy();
            return;
        }

        const acceptHash = crypto
            .createHash('sha1')
            .update(key + WS_MAGIC)
            .digest('base64');

        const headers = [
            'HTTP/1.1 101 Switching Protocols',
            'Upgrade: websocket',
            'Connection: Upgrade',
            `Sec-WebSocket-Accept: ${acceptHash}`
        ];

        socket.write(headers.join('\r\n') + '\r\n\r\n');

        const client = new WebSocketConnection(socket, isSecure);
        this.clients.add(client);

        if (head && head.length > 0) {
            client._onData(head);
        }

        client.on('close', () => {
            this.clients.delete(client);
            this.emit('disconnect', client);
        });

        client.on('message', msg => {
            this._handleClientMessage(client, msg);
        });

        this.emit('connection', client);
    }

    _handleClientMessage(client, raw) {
        try {
            const data = JSON.parse(raw);
            const { type, action, topic, payload } = data;

            // Handle ping/latency measurement
            if (type === 'PING' || action === 'ping') {
                client.send({
                    type: 'PONG',
                    timestamp: data.timestamp || Date.now(),
                    serverTime: Date.now()
                });
                return;
            }

            // Handle subscription
            if (type === 'SUBSCRIBE' || action === 'subscribe') {
                const targetTopic = topic || data.channel;
                if (targetTopic) {
                    client.subscriptions.add(targetTopic);
                    client.send({
                        type: 'SUBSCRIBED',
                        topic: targetTopic,
                        status: 'OK'
                    });
                }
                return;
            }

            if (type === 'UNSUBSCRIBE' || action === 'unsubscribe') {
                const targetTopic = topic || data.channel;
                if (targetTopic) {
                    client.subscriptions.delete(targetTopic);
                    client.send({
                        type: 'UNSUBSCRIBED',
                        topic: targetTopic,
                        status: 'OK'
                    });
                }
                return;
            }

            this.emit('message', { client, data });
        } catch (err) {
            client.send({ type: 'ERROR', message: 'Malformed JSON payload' });
        }
    }

    /**
     * Broadcast to all connected clients or those subscribed to a topic
     */
    broadcast(topic, message) {
        if (typeof message === 'object') {
            message = JSON.stringify(message);
        }
        for (const client of this.clients) {
            if (!topic || client.subscriptions.has(topic) || client.subscriptions.has('global')) {
                client.send(message);
            }
        }
    }

    broadcastAll(message) {
        this.broadcast(null, message);
    }

    close() {
        clearInterval(this.heartbeatTimer);
        for (const client of this.clients) {
            client.close(1001, 'Server shutting down');
        }
        this.clients.clear();
    }
}

const wss = new WebSocketServer();

module.exports = wss;
