/**
 * NauraLauncher Production Backend Server
 * High security, TLS 1.3 / 1.2, WebSocket Secure (WSS), MySQL connection layer.
 */

const http = require('http');
const https = require('https');
const fs = require('fs');
const path = require('path');
const router = require('./routes');
const db = require('./db');
const wss = require('./websocket');

const HTTP_PORT = parseInt(process.env.PORT || '8080', 10);
const HTTPS_PORT = parseInt(process.env.TLS_PORT || '8443', 10);
const HOST = '0.0.0.0';

async function startServer() {
    console.log('====================================================');
    console.log('  APEX / NauraLauncher Production Backend Service   ');
    console.log('====================================================');

    // 1. Initialize Database
    await db.initialize();

    // 2. Load or Verify TLS Certificates
    let tlsOptions = null;
    const certPath = path.join(__dirname, '../certs/server.crt');
    const keyPath = path.join(__dirname, '../certs/server.key');

    if (fs.existsSync(certPath) && fs.existsSync(keyPath)) {
        tlsOptions = {
            key: fs.readFileSync(keyPath),
            cert: fs.readFileSync(certPath),
            minVersion: 'TLSv1.2',
            ciphers: [
                'ECDHE-ECDSA-AES128-GCM-SHA256',
                'ECDHE-RSA-AES128-GCM-SHA256',
                'ECDHE-ECDSA-AES256-GCM-SHA384',
                'ECDHE-RSA-AES256-GCM-SHA384'
            ].join(':'),
            honorCipherOrder: true
        };
        console.log('[TLS] Loaded X.509 SSL Certificate and Private Key.');
    } else {
        console.warn('[TLS] Certificates not found in certs/ directory; starting in HTTP-only mode.');
    }

    // 3. Create HTTP Server (for previews / local proxy)
    const httpServer = http.createServer((req, res) => {
        router(req, res);
    });

    httpServer.on('upgrade', (req, socket, head) => {
        if (req.url === '/ws' || req.url?.startsWith('/ws?')) {
            wss.handleUpgrade(req, socket, head, false);
        } else {
            socket.destroy();
        }
    });

    httpServer.listen(HTTP_PORT, HOST, () => {
        console.log(`[HTTP/WS] Server listening on http://${HOST}:${HTTP_PORT} (WS: ws://${HOST}:${HTTP_PORT}/ws)`);
    });

    // 4. Create HTTPS / TLS Server if certificates are available
    let httpsServer = null;
    if (tlsOptions) {
        httpsServer = https.createServer(tlsOptions, (req, res) => {
            router(req, res);
        });

        httpsServer.on('upgrade', (req, socket, head) => {
            if (req.url === '/ws' || req.url?.startsWith('/ws?')) {
                wss.handleUpgrade(req, socket, head, true);
            } else {
                socket.destroy();
            }
        });

        httpsServer.listen(HTTPS_PORT, HOST, () => {
            console.log(`[HTTPS/WSS] Secure TLS Server listening on https://${HOST}:${HTTPS_PORT} (WSS: wss://${HOST}:${HTTPS_PORT}/ws)`);
        });
    }

    // Graceful shutdown
    const shutdown = () => {
        console.log('[Server] Shutting down gracefully...');
        wss.close();
        httpServer.close(() => {
            if (httpsServer) {
                httpsServer.close(() => process.exit(0));
            } else {
                process.exit(0);
            }
        });
    };

    process.on('SIGINT', shutdown);
    process.on('SIGTERM', shutdown);
}

if (require.main === module) {
    startServer().catch(err => {
        console.error('[Server] Fatal startup error:', err);
        process.exit(1);
    });
}

module.exports = { startServer };
