const crypto = require('crypto');

/**
 * AES-256-GCM Encryption Utility
 * Provides symmetric encryption for sensitive game data
 *
 * GCM mode includes built-in authentication (AEAD - Authenticated Encryption with Associated Data)
 * This prevents tampering while keeping data private.
 */

const ALGORITHM = 'aes-256-gcm';
const KEY_LENGTH = 32; // 256 bits
const IV_LENGTH = 16;  // 128 bits
const AUTH_TAG_LENGTH = 16; // 128 bits

/**
 * Derive a consistent 32-byte key from a master secret
 * @param {string} masterSecret - The master secret (should come from env var)
 * @returns {Buffer} 32-byte key for AES-256
 */
function deriveKey(masterSecret) {
  // Use SHA-256 to derive a consistent key from the master secret
  return crypto
    .createHash('sha256')
    .update(masterSecret)
    .digest();
}

/**
 * Encrypt sensitive data (e.g., save game data)
 * Returns format: IV + AuthTag + Ciphertext (all hex encoded)
 *
 * @param {string|object} data - Data to encrypt (will be JSON stringified if object)
 * @param {string} masterSecret - Master encryption secret
 * @returns {string} Encrypted data in format: `{iv}.{authTag}.{ciphertext}` (hex encoded)
 */
function encrypt(data, masterSecret) {
  try {
    // Generate random IV for each encryption
    const iv = crypto.randomBytes(IV_LENGTH);
    const key = deriveKey(masterSecret);

    // Convert data to JSON if it's an object
    const plaintext =
      typeof data === 'string' ? data : JSON.stringify(data);

    // Create cipher
    const cipher = crypto.createCipheriv(ALGORITHM, key, iv);

    // Encrypt the data
    let ciphertext = cipher.update(plaintext, 'utf8', 'hex');
    ciphertext += cipher.final('hex');

    // Get authentication tag
    const authTag = cipher.getAuthTag();

    // Return: IV.AuthTag.Ciphertext (all hex)
    return `${iv.toString('hex')}.${authTag.toString('hex')}.${ciphertext}`;
  } catch (err) {
    throw new Error(`Encryption failed: ${err.message}`);
  }
}

/**
 * Decrypt sensitive data
 * Expects format: IV + AuthTag + Ciphertext (hex encoded)
 *
 * @param {string} encryptedData - Encrypted data in format: `{iv}.{authTag}.{ciphertext}`
 * @param {string} masterSecret - Master encryption secret
 * @returns {object} Decrypted data parsed as JSON
 */
function decrypt(encryptedData, masterSecret) {
  try {
    // Parse the encrypted data format
    const parts = encryptedData.split('.');
    if (parts.length !== 3) {
      throw new Error('Invalid encrypted data format');
    }

    const iv = Buffer.from(parts[0], 'hex');
    const authTag = Buffer.from(parts[1], 'hex');
    const ciphertext = parts[2];

    // Validate lengths
    if (iv.length !== IV_LENGTH) {
      throw new Error('Invalid IV length');
    }
    if (authTag.length !== AUTH_TAG_LENGTH) {
      throw new Error('Invalid authentication tag length');
    }

    const key = deriveKey(masterSecret);

    // Create decipher
    const decipher = crypto.createDecipheriv(ALGORITHM, key, iv);
    decipher.setAuthTag(authTag);

    // Decrypt
    let plaintext = decipher.update(ciphertext, 'hex', 'utf8');
    plaintext += decipher.final('utf8');

    // Parse JSON
    return JSON.parse(plaintext);
  } catch (err) {
    throw new Error(`Decryption failed: ${err.message}`);
  }
}

/**
 * Encrypt data and return as Base64 string (URL-safe for API transmission)
 * Useful when you need to send encrypted data in HTTP headers or query params
 *
 * @param {string|object} data - Data to encrypt
 * @param {string} masterSecret - Master encryption secret
 * @returns {string} Encrypted data as Base64 URL-safe string
 */
function encryptToBase64(data, masterSecret) {
  const encrypted = encrypt(data, masterSecret);
  return Buffer.from(encrypted).toString('base64url');
}

/**
 * Decrypt data from Base64 string
 *
 * @param {string} base64Data - Base64 URL-safe encrypted data
 * @param {string} masterSecret - Master encryption secret
 * @returns {object} Decrypted data
 */
function decryptFromBase64(base64Data, masterSecret) {
  const encrypted = Buffer.from(base64Data, 'base64url').toString();
  return decrypt(encrypted, masterSecret);
}

module.exports = {
  encrypt,
  decrypt,
  encryptToBase64,
  decryptFromBase64,
  deriveKey,
  ALGORITHM,
  KEY_LENGTH,
  IV_LENGTH,
  AUTH_TAG_LENGTH
};
