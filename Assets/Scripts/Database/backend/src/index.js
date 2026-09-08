const express = require('express');
const cors = require('cors');
const config = require('./config');
const { logMissionResult, getMissionLogs, getLeaderboardTop } = require('./db/supabase');
const authRouter = require('./routes/auth');
const { authenticateToken } = require('./middleware/auth');

// Validate config at startup
config.validate();

const app = express();

// Middleware
app.use(cors()); // Enable CORS for all routes
app.use(express.json());

// Mount authentication routes
app.use('/auth', authRouter);

/**
 * POST /v1/saves
 * Save mission result (game_logs table)
 * PROTECTED: Requires JWT token in Authorization header
 *
 * Request body: { playerName, firesExtinguished, duration }
 * Headers: Authorization: Bearer <jwt_token>
 * Response: { success: true, data: {...}, error: null }
 */
app.post('/v1/saves', authenticateToken, async (req, res) => {
  try {
    const { playerName, firesExtinguished, duration } = req.body;

    if (!playerName || firesExtinguished === undefined || duration === undefined) {
      return res.status(400).json({
        success: false,
        error: 'Missing playerName, firesExtinguished, or duration',
        data: null
      });
    }

    const result = await logMissionResult(playerName, firesExtinguished, duration);

    return res.status(200).json({
      success: true,
      data: result,
      error: null
    });
  } catch (err) {
    console.error('[POST /v1/saves Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Failed to save mission result',
      data: null
    });
  }
});

/**
 * GET /v1/saves
 * Retrieve all mission logs
 *
 * Query param: limit (default 100)
 * Response: { success: true, data: [...], error: null }
 */
app.get('/v1/saves', async (req, res) => {
  try {
    const limit = Math.min(parseInt(req.query.limit || '100'), 100);

    const result = await getMissionLogs(limit);

    return res.status(200).json({
      success: true,
      data: result,
      error: null
    });
  } catch (err) {
    console.error('[GET /v1/saves Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Failed to retrieve mission logs',
      data: null
    });
  }
});

/**
 * GET /v1/leaderboard/top
 * Fetch top players by fires extinguished (global leaderboard)
 *
 * Query param: limit (default 10, max 100)
 * Response: { success: true, data: [...], error: null }
 */
app.get('/v1/leaderboard/top', async (req, res) => {
  try {
    const limit = Math.min(parseInt(req.query.limit || '10'), 100);

    const result = await getLeaderboardTop(limit);

    return res.status(200).json({
      success: true,
      data: result,
      error: null
    });
  } catch (err) {
    console.error('[GET /v1/leaderboard/top Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Failed to retrieve leaderboard',
      data: null
    });
  }
});

/**
 * Health check endpoint
 */
app.get('/health', (req, res) => {
  return res.status(200).json({ status: 'ok' });
});

/**
 * 404 handler
 */
app.use((req, res) => {
  return res.status(404).json({
    success: false,
    error: 'Endpoint not found',
    data: null
  });
});

/**
 * Error handler
 */
app.use((err, req, res, next) => {
  console.error('[Unhandled Error]', err.message);
  return res.status(500).json({
    success: false,
    error: 'Internal server error',
    data: null
  });
});

// Start server
const PORT = config.PORT;
app.listen(PORT, () => {
  console.log(`🚀 Server running on port ${PORT} (${config.NODE_ENV})`);
});

module.exports = app;
