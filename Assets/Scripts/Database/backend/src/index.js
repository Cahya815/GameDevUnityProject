const express = require('express');
const cors = require('cors');
const path = require('path');
const config = require('./config');
const { logMissionResult, getMissionLogs, getLeaderboardTop } = require('./db/supabase');
const authRouter = require('./routes/auth');
const profileRouter = require('./routes/profile');
const { authenticateToken } = require('./middleware/auth');
const azureBlob = require('./storage/azureBlob');

// Validate config at startup
config.validate();

const app = express();

// Middleware
app.use(cors()); // Enable CORS for all routes
app.use(express.json());

// Serve static frontend files (Web Dashboard)
app.use(express.static(path.join(__dirname, 'public')));

// Root route serves Web Dashboard
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Mount authentication & profile routes
app.use('/auth', authRouter);
app.use('/v1/profile', profileRouter);

/**
 * POST /v1/saves
 * Save mission result (game_logs table)
 * PROTECTED: Requires JWT token in Authorization header
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
  return res.status(200).json({
    status: 'ok',
    storage: {
      provider: 'azure-blob',
      container: config.AZURE_CONTAINER_NAME || 'avatars'
    },
    timestamp: new Date().toISOString()
  });
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
    error: 'Internal server error: ' + err.message,
    data: null
  });
});

// Start server
const PORT = config.PORT;
app.listen(PORT, async () => {
  console.log(`🚀 Server running on port ${PORT} (${config.NODE_ENV})`);
  // Try to initialize Azure storage container
  try {
    await azureBlob.ensureContainerExists();
  } catch (e) {
    // Non-blocking
  }
});

module.exports = app;
