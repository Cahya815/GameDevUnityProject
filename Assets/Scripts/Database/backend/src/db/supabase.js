const { createClient } = require('@supabase/supabase-js');
const config = require('../config');

let supabase = null;

/**
 * Lazy-initialize Supabase client with service role
 * Service role bypasses RLS — only called server-side by Cloud Run
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
 * Log mission result to game_logs table
 * @param {string} playerName - Player name
 * @param {number} firesExtinguished - Number of fires extinguished
 * @param {number} duration - Mission duration in seconds
 * @returns {object} - Inserted game_logs row
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
 * @param {number} limit - Max results (default 100)
 * @returns {array} - Array of game_logs rows
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
 * @param {number} limit - Max results (default 10)
 * @returns {array} - Sorted by fires_extinguished DESC, duration ASC
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
  logMissionResult,
  getMissionLogs,
  getLeaderboardTop
};