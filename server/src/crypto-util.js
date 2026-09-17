/**
 * NauraLauncher Cryptographic Utility Module
 * Production-grade security: PBKDF2-SHA256, HMAC-SHA256 JWT, Anti-Tamper Signatures
 */

const crypto = require('crypto');
const fs = require('fs');

const JWT_SECRET = process.env.JWT_SECRET || 'apex-protocol-super-secure-jwt-secret-key-2026-x99!';
const HMAC_KEY = process.env.HMAC_KEY || 'apex-protocol-anti-tamper-hmac-launch-key-2026';

/**
 * Hash password using PBKDF2-SHA256 with 100,000 iterations.
 */
function hashPassword(password, salt = null) {
    if (!salt) {
        salt = crypto.randomBytes(16).toString('hex');
    }
    const hash = crypto.pbkdf2Sync(password, salt, 100000, 64, 'sha256').toString('hex');
    return { hash, salt };
}

/**
 * Verify password against salt and expected hash using constant-time comparison.
 */
function verifyPassword(password, salt, expectedHash) {
    const { hash } = hashPassword(password, salt);
    const hashBuf = Buffer.from(hash, 'hex');
    const expectedBuf = Buffer.from(expectedHash, 'hex');
    if (hashBuf.length !== expectedBuf.length) return false;
    return crypto.timingSafeEqual(hashBuf, expectedBuf);
}

/**
 * Create a signed JWT token (HMAC-SHA256)
 */
function createJwt(payload, expiresInSeconds = 3600) {
    const header = {
        alg: 'HS256',
        typ: 'JWT'
    };
    const now = Math.floor(Date.now() / 1000);
    const exp = now + expiresInSeconds;
    const fullPayload = {
        ...payload,
        iat: now,
        exp: exp
    };

    const encodedHeader = Buffer.from(JSON.stringify(header)).toString('base64url');
    const encodedPayload = Buffer.from(JSON.stringify(fullPayload)).toString('base64url');
    const signature = crypto
        .createHmac('sha256', JWT_SECRET)
        .update(`${encodedHeader}.${encodedPayload}`)
        .digest('base64url');

    return `${encodedHeader}.${encodedPayload}.${signature}`;
}

/**
 * Verify and decode a JWT token
 */
function verifyJwt(token) {
    if (!token || typeof token !== 'string') return null;
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const [encodedHeader, encodedPayload, signature] = parts;
    const expectedSignature = crypto
        .createHmac('sha256', JWT_SECRET)
        .update(`${encodedHeader}.${encodedPayload}`)
        .digest('base64url');

    const sigBuf = Buffer.from(signature);
    const expBuf = Buffer.from(expectedSignature);
    if (sigBuf.length !== expBuf.length || !crypto.timingSafeEqual(sigBuf, expBuf)) {
        return null;
    }

    try {
        const payload = JSON.parse(Buffer.from(encodedPayload, 'base64url').toString('utf8'));
        const now = Math.floor(Date.now() / 1000);
        if (payload.exp && payload.exp < now) {
            return null; // Expired
        }
        return payload;
    } catch {
        return null;
    }
}

/**
 * Generate cryptographically secure random token (e.g. for refresh tokens or session IDs)
 */
function generateRandomToken(bytes = 32) {
    return crypto.randomBytes(bytes).toString('hex');
}

/**
 * Compute SHA-256 hash of a string or buffer
 */
function sha256(data) {
    return crypto.createHash('sha256').update(data).digest('hex');
}

/**
 * Compute SHA-256 hash of a file on disk
 */
function hashFile(filePath) {
    return new Promise((resolve, reject) => {
        const hash = crypto.createHash('sha256');
        const stream = fs.createReadStream(filePath);
        stream.on('data', chunk => hash.update(chunk));
        stream.on('end', () => resolve(hash.digest('hex')));
        stream.on('error', reject);
    });
}

/**
 * Sign launch payload with HMAC for anti-tampering
 */
function signLaunchPayload(payload) {
    const serialized = JSON.stringify(payload);
    const signature = crypto.createHmac('sha256', HMAC_KEY).update(serialized).digest('hex');
    return { payload, signature };
}

/**
 * Verify launch payload signature
 */
function verifyLaunchSignature(payload, signature) {
    const serialized = JSON.stringify(payload);
    const expected = crypto.createHmac('sha256', HMAC_KEY).update(serialized).digest('hex');
    const sigBuf = Buffer.from(signature, 'hex');
    const expBuf = Buffer.from(expected, 'hex');
    if (sigBuf.length !== expBuf.length) return false;
    return crypto.timingSafeEqual(sigBuf, expBuf);
}

module.exports = {
    hashPassword,
    verifyPassword,
    createJwt,
    verifyJwt,
    generateRandomToken,
    sha256,
    hashFile,
    signLaunchPayload,
    verifyLaunchSignature
};
