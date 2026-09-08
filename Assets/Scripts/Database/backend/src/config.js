require('dotenv').config();

module.exports = {
  PORT: process.env.PORT || 3000,
  NODE_ENV: process.env.NODE_ENV || 'development',

  // Supabase
  SUPABASE_URL: process.env.SUPABASE_URL,
  SUPABASE_SERVICE_ROLE_KEY: process.env.SUPABASE_SERVICE_ROLE_KEY,

  // JWT Authentication
  JWT_SECRET: process.env.JWT_SECRET || 'default-jwt-secret-change-in-production',
  JWT_EXPIRES_IN: process.env.JWT_EXPIRES_IN || '7d',

  // AES Encryption
  ENCRYPTION_SECRET: process.env.ENCRYPTION_SECRET || 'default-encryption-secret-change-in-production',

  // Basic validation 
  validate() {
    const required = ['SUPABASE_URL', 'SUPABASE_SERVICE_ROLE_KEY'];
    const missing = required.filter(key => !process.env[key]);
    if (missing.length) {
      throw new Error(`Missing required env vars: ${missing.join(', ')}`);
    }

    // Warn if using default secrets (only in development)
    if (process.env.NODE_ENV === 'development') {
      if (process.env.JWT_SECRET === 'default-jwt-secret-change-in-production') {
        console.warn('[WARN] Using default JWT_SECRET - change for production!');
      }
      if (process.env.ENCRYPTION_SECRET === 'default-encryption-secret-change-in-production') {
        console.warn('[WARN] Using default ENCRYPTION_SECRET - change for production!');
      }
    }
  }
};