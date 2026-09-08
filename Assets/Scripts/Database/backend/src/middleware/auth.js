const jwt = require('jsonwebtoken');
const config = require('../config');

/**
 * JWT Authentication Middleware
 *
 * Verifies JWT token from Authorization header and attaches user info to req.user
 *
 * Usage:
 *   app.post('/v1/saves', authenticateToken, async (req, res) => {
 *     const userId = req.user.userId; // Access authenticated user
 *     ...
 *   });
 *
 * Expected header format:
 *   Authorization: Bearer <jwt_token>
 */

/**
 * Middleware to authenticate JWT token
 * Extracts token from Authorization header, verifies it, and attaches user to req
 */
function authenticateToken(req, res, next) {
  try {
    // Extract token from Authorization header
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1]; // Format: "Bearer TOKEN"

    if (!token) {
      return res.status(401).json({
        success: false,
        error: 'Access token required',
        data: null
      });
    }

    // Verify token
    jwt.verify(token, config.JWT_SECRET, (err, decoded) => {
      if (err) {
        // Token is invalid or expired
        if (err.name === 'TokenExpiredError') {
          return res.status(403).json({
            success: false,
            error: 'Token expired',
            data: null
          });
        }

        return res.status(403).json({
          success: false,
          error: 'Invalid token',
          data: null
        });
      }

      // Attach user info to request object
      req.user = {
        userId: decoded.userId,
        username: decoded.username
      };

      next(); // Proceed to the route handler
    });
  } catch (err) {
    console.error('[Auth Middleware Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Authentication failed',
      data: null
    });
  }
}

/**
 * Optional middleware - authenticates token if present, but doesn't require it
 * Useful for endpoints that work both authenticated and unauthenticated
 */
function authenticateTokenOptional(req, res, next) {
  try {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];

    if (!token) {
      // No token present, proceed without authentication
      req.user = null;
      return next();
    }

    // Token present, verify it
    jwt.verify(token, config.JWT_SECRET, (err, decoded) => {
      if (err) {
        // Invalid token, but we allow the request to proceed
        req.user = null;
      } else {
        // Valid token, attach user info
        req.user = {
          userId: decoded.userId,
          username: decoded.username
        };
      }
      next();
    });
  } catch (err) {
    console.error('[Auth Optional Middleware Error]', err.message);
    req.user = null;
    next();
  }
}

/**
 * Generate JWT token for a user
 * Call this after successful login/register
 *
 * @param {object} payload - { userId, username }
 * @param {string} expiresIn - Token expiration (e.g., '7d', '24h', '1h')
 * @returns {string} JWT token
 */
function generateToken(payload, expiresIn = '7d') {
  return jwt.sign(payload, config.JWT_SECRET, {
    expiresIn,
    issuer: 'game-pemadam-api',
    audience: 'game-pemadam-client'
  });
}

/**
 * Verify and decode a JWT token without middleware context
 * Useful for manual token validation
 *
 * @param {string} token - JWT token to verify
 * @returns {object|null} Decoded payload or null if invalid
 */
function verifyToken(token) {
  try {
    return jwt.verify(token, config.JWT_SECRET);
  } catch (err) {
    return null;
  }
}

module.exports = {
  authenticateToken,
  authenticateTokenOptional,
  generateToken,
  verifyToken
};
