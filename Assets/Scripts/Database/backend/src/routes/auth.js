const express = require('express');
const bcrypt = require('bcryptjs');
const { getSupabase } = require('../db/supabase');
const { authenticateToken, generateToken } = require('../middleware/auth');
const config = require('../config');

const router = express.Router();

/**
 * Ensure the `users` table exists in Supabase.
 * If it does not, the developer must create it manually (see comments).
 */
async function ensureUsersTableExists() {
  const sb = getSupabase();
  const { error } = await sb.from('users').select('id').limit(1).single();
  if (error && error.message && error.message.includes('does not exist')) {
    console.warn('[WARN] Supabase `users` table missing. Create it via SQL Editor.');
  }
}

/**
 * POST /auth/register – Register a new user.
 * Body: { email, username, password }
 */
router.post('/register', async (req, res) => {
  try {
    const { email, username, password } = req.body;
    if (!email || !username || !password) {
      return res.status(400).json({ success: false, data: null, error: 'Missing email, username, or password' });
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      return res.status(400).json({ success: false, data: null, error: 'Invalid email format' });
    }
    if (password.length < 8) {
      return res.status(400).json({ success: false, data: null, error: 'Password must be at least 8 characters' });
    }
    await ensureUsersTableExists();
    const sb = getSupabase();

    // Check if username already exists
    const { data: existingUser, error: findErr } = await sb.from('users').select('username').eq('username', username).single();
    if (existingUser) {
      return res.status(400).json({ success: false, data: null, error: 'Username sudah terpakai' });
    }
    if (findErr && findErr.code !== 'PGRST116') { // ignore not‑found error
      throw findErr;
    }

    const passwordHash = await bcrypt.hash(password, 10);
    const { data: newUser, error: insertErr } = await sb
      .from('users')
      .insert({ email: email.toLowerCase(), username, password_hash: passwordHash })
      .select('id, email, username, created_at')
      .single();
    if (insertErr) throw insertErr;

    const token = generateToken({ userId: newUser.id, username: newUser.username }, config.JWT_EXPIRES_IN);
    return res.status(201).json({
      success: true,
      data: { userId: newUser.id, email: newUser.email, username: newUser.username, token },
      error: null
    });
  } catch (err) {
    console.error('[POST /auth/register] ', err.message);
    return res.status(500).json({ success: false, data: null, error: 'Registration failed' });
  }
});

/**
 * POST /auth/login – Log in a user.
 * Body: { email, password }
 */
router.post('/login', async (req, res) => {
  try {
    const { email, password } = req.body;
    if (!email || !password) {
      return res.status(400).json({ success: false, data: null, error: 'Missing email or password' });
    }
    await ensureUsersTableExists();
    const sb = getSupabase();
    const { data: user, error } = await sb
      .from('users')
      .select('id, username, email, password_hash')
      .ilike('email', email.toLowerCase())
      .single();
    if (error || !user) {
      return res.status(401).json({ success: false, data: null, error: 'User tidak ditemukan, silakan register' });
    }
    const valid = await bcrypt.compare(password, user.password_hash);
    if (!valid) {
      return res.status(401).json({ success: false, data: null, error: 'Password Salah' });
    }
    await sb.from('users').update({ last_login: new Date() }).eq('id', user.id);
    const token = generateToken({ userId: user.id, username: user.username }, config.JWT_EXPIRES_IN);
    return res.status(200).json({
      success: true,
      data: { userId: user.id, username: user.username, email: user.email, token },
      error: null
    });
  } catch (err) {
    console.error('[POST /auth/login] ', err.message);
    return res.status(500).json({ success: false, data: null, error: 'Login failed' });
  }
});

/**
 * GET /auth/me – Get current authenticated user profile.
 */
router.get('/me', authenticateToken, async (req, res) => {
  try {
    const sb = getSupabase();
    const { data: user, error } = await sb
      .from('users')
      .select('id, username, email, created_at, last_login')
      .eq('id', req.user.userId)
      .single();
    if (error) throw error;
    return res.status(200).json({ success: true, data: user, error: null });
  } catch (err) {
    console.error('[GET /auth/me] ', err.message);
    return res.status(500).json({ success: false, data: null, error: 'Failed to get user data' });
  }
});

module.exports = router;
