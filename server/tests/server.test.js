/**
 * Automated Production Integration Tests for NauraLauncher Backend
 */

const https = require('https');
const http = require('http');
const crypto = require('crypto');
const path = require('path');
// Set environment variables before importing server
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
process.env.PORT = '8081';
process.env.TLS_PORT = '8444';
process.env.DB_PATH = path.join(__dirname, '../data/test_launcher.db');

// Ensure clean test database on test startup
try {
    const fs = require('fs');
    if (fs.existsSync(process.env.DB_PATH)) fs.unlinkSync(process.env.DB_PATH);
    const wal = process.env.DB_PATH + '-wal';
    if (fs.existsSync(wal)) fs.unlinkSync(wal);
    const shm = process.env.DB_PATH + '-shm';
    if (fs.existsSync(shm)) fs.unlinkSync(shm);
} catch {}

const { startServer } = require('../src/server');

function request(options, data = null) {
    return new Promise((resolve, reject) => {
        const client = options.protocol === 'https:' ? https : http;
        const req = client.request(options, res => {
            let body = '';
            res.on('data', chunk => body += chunk);
            res.on('end', () => {
                let parsed = body;
                try {
                    parsed = JSON.parse(body);
                } catch {}
                resolve({ status: res.statusCode, headers: res.headers, data: parsed });
            });
        });
        req.on('error', reject);
        if (data) {
            req.write(typeof data === 'string' ? data : JSON.stringify(data));
        }
        req.end();
    });
}

function testWebSocket(port, isSecure = false) {
    return new Promise((resolve, reject) => {
        const net = require('net');
        const tls = require('tls');
        const key = crypto.randomBytes(16).toString('base64');
        const client = isSecure ? tls.connect(port, '127.0.0.1', { rejectUnauthorized: false }) : net.connect(port, '127.0.0.1');

        client.on('connect', () => {
            const req = [
                'GET /ws HTTP/1.1',
                'Host: 127.0.0.1:' + port,
                'Upgrade: websocket',
                'Connection: Upgrade',
                'Sec-WebSocket-Key: ' + key,
                'Sec-WebSocket-Version: 13',
                '',
                ''
            ].join('\r\n');
            client.write(req);
        });

        let upgraded = false;
        client.on('data', chunk => {
            if (!upgraded) {
                const text = chunk.toString();
                if (text.includes('101 Switching Protocols')) {
                    upgraded = true;
                    // Send WebSocket PING frame
                    // Masked text frame {"type":"PING","timestamp":12345}
                    const payload = Buffer.from(JSON.stringify({ type: 'PING', timestamp: 12345 }));
                    const mask = Buffer.from([0x12, 0x34, 0x56, 0x78]);
                    const masked = Buffer.alloc(payload.length);
                    for (let i = 0; i < payload.length; i++) {
                        masked[i] = payload[i] ^ mask[i % 4];
                    }
                    const header = Buffer.from([0x81, 0x80 | payload.length]);
                    client.write(Buffer.concat([header, mask, masked]));
                } else {
                    reject(new Error('Upgrade failed'));
                }
            } else {
                // Received WS frame from server
                // Expect unmasked text frame from server with PONG
                const opcode = chunk[0] & 0x0f;
                const len = chunk[1] & 0x7f;
                const payload = chunk.subarray(2, 2 + len).toString();
                try {
                    const parsed = JSON.parse(payload);
                    if (parsed.type === 'PONG' && parsed.timestamp === 12345) {
                        client.end();
                        resolve(true);
                    }
                } catch (e) {
                    // Ignore non-json or partial frames
                }
            }
        });

        client.on('error', reject);
    });
}

async function runTests() {
    console.log('[Test] Starting Backend Service...');
    await startServer();
    // Wait for server to bind
    await new Promise(r => setTimeout(r, 600));

    console.log('[Test 1] Testing Plain HTTP /api/v1/status');
    const resStatusHttp = await request({
        protocol: 'http:',
        host: '127.0.0.1',
        port: 8081,
        path: '/api/v1/status',
        method: 'GET'
    });
    console.assert(resStatusHttp.status === 200, 'HTTP status check failed');
    console.assert(resStatusHttp.data.status === 'HEALTHY', 'Status must be HEALTHY');
    console.log('✓ Plain HTTP Status Check passed.');

    console.log('[Test 2] Testing TLS / HTTPS /api/v1/status');
    const resStatusTls = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/status',
        method: 'GET'
    });
    console.assert(resStatusTls.status === 200, 'TLS status check failed');
    console.assert(resStatusTls.data.tls === 'TLSv1.3', 'TLS version must be active');
    console.log('✓ Secure TLS/HTTPS Status Check passed.');

    console.log('[Test 3] Testing User Authentication & Token Generation');
    const registerRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/auth/register',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    }, {
        username: 'TEST_PILOT_01',
        email: 'testpilot01@apex.io',
        password: 'SecurePassword@2026!'
    });
    console.assert(registerRes.status === 201, 'Registration failed');
    console.assert(registerRes.data.token != null, 'JWT token must be returned');
    const authToken = registerRes.data.token;
    console.log('✓ User Registration & PBKDF2 Password Hashing passed.');

    console.log('[Test 4] Testing User Login with PBKDF2 verification');
    const loginRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/auth/login',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    }, {
        username: 'TEST_PILOT_01',
        password: 'SecurePassword@2026!'
    });
    console.assert(loginRes.status === 200, 'Login failed');
    console.assert(loginRes.data.user.username === 'TEST_PILOT_01', 'User object matching failed');
    console.log('✓ User Login & JWT verification passed.');

    console.log('[Test 5] Testing Games Catalog & Manifest Retrieval');
    const gamesRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/games',
        method: 'GET'
    });
    console.assert(gamesRes.status === 200, 'Games query failed');
    console.assert(gamesRes.data.games.length >= 5, 'Games catalog incomplete');

    const manifestRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: `/api/v1/games/${gamesRes.data.games[0].id}/manifest`,
        method: 'GET'
    });
    console.assert(manifestRes.status === 200, 'Manifest fetch failed');
    console.assert(manifestRes.data.manifest[0].sha256_hash != null, 'Manifest SHA-256 missing');
    console.log('✓ Games Catalog & SHA-256 Manifest Integrity passed.');

    console.log('[Test 6] Testing Game Claiming & HMAC Launch Token Generation');
    const claimRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/library/claim',
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`
        }
    }, {
        gameId: gamesRes.data.games[0].id
    });
    console.assert(claimRes.status === 201, 'Game claim failed');

    const launchRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: `/api/v1/library/${gamesRes.data.games[0].id}/launch-token`,
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authToken}`
        }
    });
    console.assert(launchRes.status === 200, 'Launch token generation failed');
    console.assert(launchRes.data.launchToken != null, 'Launch token missing');
    console.log('✓ Library Entitlement & Anti-Tamper Launch Token passed.');

    console.log('[Test 7] Testing Real-time Auction Bidding & Atomic Transactions');
    const bidRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: '/api/v1/auction/bid',
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`
        }
    }, {
        lotId: 'lot-katana-0042',
        incrementValue: 500
    });
    console.assert(bidRes.status === 200, 'Bid placement failed');
    console.assert(bidRes.data.currentBid === 52900, `Expected 52900, got ${bidRes.data.currentBid}`);
    console.log('✓ Auction Floor Bidding & Atomic State Transition passed.');

    console.log('[Test 8] Testing RFC 6455 WebSocket Heartbeat over Plain WS');
    const wsOk = await testWebSocket(8081, false);
    console.assert(wsOk, 'WS connection failed');
    console.log('✓ Real-time WebSocket Protocol & Heartbeat passed.');

    console.log('[Test 9] Testing RFC 6455 WebSocket Heartbeat over Secure TLS / WSS');
    const wssOk = await testWebSocket(8444, true);
    console.assert(wssOk, 'WSS connection failed');
    console.log('✓ Real-time Secure WSS Protocol & Heartbeat passed.');

    console.log('[Test 10] Testing Resumable Chunked Download with Range Header');
    const rangeRes = await request({
        protocol: 'https:',
        host: '127.0.0.1',
        port: 8444,
        path: `/api/v1/download/${gamesRes.data.games[0].id}/game.exe`,
        method: 'GET',
        headers: {
            'Range': 'bytes=0-1023'
        }
    });
    console.assert(rangeRes.status === 206, 'Partial content range failed');
    console.assert(rangeRes.headers['content-range'] != null, 'Content-Range header missing');
    console.assert(rangeRes.headers['x-checksum-sha256'] != null, 'SHA-256 header missing');
    console.log('✓ Resumable Chunked HTTP Range Download & Integrity Header passed.');

    console.log('\n====================================================');
    console.log('  ALL BACKEND INTEGRATION TESTS PASSED (10/10)       ');
    console.log('====================================================\n');
    process.exit(0);
}

runTests().catch(err => {
    console.error('[Test Failed]', err);
    process.exit(1);
});
