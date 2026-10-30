const { createClient } = require('@supabase/supabase-js');
const config = require('../config');

let supabase = null;

/**
 * Lazy-initialize Supabase client with service role
 * Service role bypasses RLS — only called server-side
 */
function getSupabase() {
  if (!supabase) {
    if (!config.SUPABASE_URL || !config.SUPABASE_SERVICE_ROLE_KEY) {
      throw new Error('Supabase config missing: SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY required');
    }
    supabase = createClient(config.SUPABASE_URL, config.SUPABASE_SERVICE_ROLE_KEY, {
      auth: { persistSession: false, autoRefreshToken: false },
      db: { schema: 'public' }
    });
  }
  return supabase;
}

/**
 * Get user profile data
 */
async function getUserProfile(userId) {
  const sb = getSupabase();
  try {
    const { data, error } = await sb
      .from('users')
      .select('id, email, username, avatar_url, created_at, last_login')
      .eq('id', userId)
      .single();

    if (error) {
      // Graceful fallback if avatar_url column is not yet migrated in Supabase
      if (error.message && error.message.includes('avatar_url')) {
        const fallback = await sb
          .from('users')
          .select('id, email, username, created_at, last_login')
          .eq('id', userId)
          .single();
        if (fallback.error) throw fallback.error;
        return { ...fallback.data, avatar_url: null };
      }
      throw error;
    }
    return data;
  } catch (err) {
    throw err;
  }
}

/**
 * Get user with password_hash for verification
 */
async function getUserWithPassword(userId) {
  const sb = getSupabase();
  const { data, error } = await sb
    .from('users')
    .select('id, email, username, password_hash')
    .eq('id', userId)
    .single();
  if (error) throw error;
  return data;
}

/**
 * Check if a username is already taken by another user
 */
async function isUsernameTaken(username, excludeUserId = null) {
  const sb = getSupabase();
  let query = sb.from('users').select('id, username').eq('username', username);
  if (excludeUserId) {
    query = query.neq('id', excludeUserId);
  }
  const { data } = await query.maybeSingle();
  return !!data;
}

/**
 * Update user nickname / username
 */
async function updateUserNickname(userId, newUsername) {
  const sb = getSupabase();
  const { data, error } = await sb
    .from('users')
    .update({ username: newUsername })
    .eq('id', userId)
    .select('id, username')
    .single();
  if (error) throw error;
  return data;
}

/**
 * Update user password_hash
 */
async function updateUserPassword(userId, passwordHash) {
  const sb = getSupabase();
  const { error } = await sb
    .from('users')
    .update({ password_hash: passwordHash })
    .eq('id', userId);
  if (error) throw error;
  return true;
}

/**
 * Update user avatar_url
 */
async function updateUserAvatar(userId, avatarUrl) {
  const sb = getSupabase();
  try {
    const { data, error } = await sb
      .from('users')
      .update({ avatar_url: avatarUrl })
      .eq('id', userId)
      .select('id, username, avatar_url')
      .single();

    if (error) {
      if (error.message && error.message.includes('avatar_url')) {
        throw new Error("Kolom 'avatar_url' belum ada di tabel 'users' Supabase. Jalankan query: ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_url TEXT DEFAULT NULL;");
      }
      throw error;
    }
    return data;
  } catch (err) {
    throw err;
  }
}

/**
 * Log mission result to game_logs table
 */
async function logMissionResult(playerName, firesExtinguished, duration) {
  const sb = getSupabase();
  const { data, error } = await sb
    .from('game_logs')
    .insert({
      player_name: playerName,
      fires_extinguished: firesExtinguished,
      duration: duration
    })
    .select()
    .single();
  if (error) throw error;
  return data;
}

/**
 * Get all mission logs (for leaderboard)
 */
async function getMissionLogs(limit = 100) {
  const sb = getSupabase();
  const { data, error } = await sb
    .from('game_logs')
    .select('player_name, fires_extinguished, duration, created_at')
    .order('fires_extinguished', { ascending: false })
    .order('duration', { ascending: true })
    .limit(Math.min(limit, 100));
  if (error) throw error;
  return data || [];
}

/**
 * Fetch top players by fires extinguished (leaderboard)
 */
async function getLeaderboardTop(limit = 10) {
  const sb = getSupabase();
  const { data, error } = await sb
    .from('game_logs')
    .select('player_name, fires_extinguished, duration, created_at')
    .order('fires_extinguished', { ascending: false })
    .order('duration', { ascending: true })
    .limit(Math.min(limit, 100));
  if (error) throw error;
  return data || [];
}

module.exports = {
  getSupabase,
  getUserProfile,
  getUserWithPassword,
  isUsernameTaken,
  updateUserNickname,
  updateUserPassword,
  updateUserAvatar,
  logMissionResult,
  getMissionLogs,
  getLeaderboardTop
};
