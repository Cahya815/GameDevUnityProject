# 🐳 Docker Setup Guide

## Quick Start

### 1. Prerequisites
- Docker Desktop installed
- `.env` file configured (copy from `.env.example`)

### 2. Start Development Server
```bash
# Build and start container
docker-compose up --build

# Or run in background
docker-compose up -d

# View logs
docker-compose logs -f api
```

### 3. Test the API
```bash
# Health check
curl http://localhost:3000/health

# Test save endpoint
curl -X POST http://localhost:3000/v1/saves \
  -H "Content-Type: application/json" \
  -d '{"playerName":"TestPlayer","firesExtinguished":10,"duration":120}'

# Get leaderboard
curl http://localhost:3000/v1/leaderboard/top?limit=10
```

### 4. Stop Container
```bash
docker-compose down

# Remove volumes too
docker-compose down -v
```

---

## Production Build

```bash
# Build production image
docker build --target production -t game-api:prod .

# Run production container
docker run -d \
  -p 3000:3000 \
  --env-file .env \
  --name game-api-prod \
  game-api:prod

# Check logs
docker logs -f game-api-prod
```

---

## Deploy to Cloud Run (Google Cloud)

```bash
# 1. Build for Cloud Run
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/game-api

# 2. Deploy
gcloud run deploy game-api \
  --image gcr.io/YOUR_PROJECT_ID/game-api \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars SUPABASE_URL=your-url,SUPABASE_SERVICE_ROLE_KEY=your-key

# 3. Get service URL
gcloud run services describe game-api --region us-central1 --format 'value(status.url)'
```

---

## Troubleshooting

### Container won't start
```bash
# Check logs
docker-compose logs api

# Common issues:
# 1. Missing .env file → Copy .env.example to .env
# 2. Invalid Supabase credentials → Check SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY
# 3. Port 3000 already in use → Change port in docker-compose.yml
```

### Hot-reload not working
```bash
# Ensure nodemon is installed
docker-compose exec api npm list nodemon

# Restart container
docker-compose restart api
```

### Database connection fails
```bash
# Test Supabase connection from container
docker-compose exec api curl https://your-project.supabase.co/rest/v1/

# Verify service role key has correct permissions
```

---

## Architecture

```
┌─────────────────────────────────────┐
│  Unity Game Client (C#)             │
│  - SupabaseProvider.cs calls API    │
└────────────┬────────────────────────┘
             │
             │ HTTPS POST/GET
             │
┌────────────▼────────────────────────┐
│  Docker Container (Node.js)         │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  Express.js API             │   │
│  │  - /v1/saves                │   │
│  │  - /v1/leaderboard/top      │   │
│  │  - /health                  │   │
│  └─────────┬───────────────────┘   │
│            │                        │
│  ┌─────────▼───────────────────┐   │
│  │  Supabase Client            │   │
│  │  (Service Role)             │   │
│  └─────────┬───────────────────┘   │
└────────────┼───────────────────────┘
             │
    ┌────────▼─────────┐
    │  Supabase Cloud  │
    │  (PostgreSQL)    │
    │  - game_logs     │
    └──────────────────┘
```

---

## Environment Variables Reference

| Variable | Required | Description |
|----------|----------|-------------|
| `PORT` | No | API port (default: 3000) |
| `NODE_ENV` | No | Environment (development/production) |
| `SUPABASE_URL` | **Yes** | Supabase project URL |
| `SUPABASE_SERVICE_ROLE_KEY` | **Yes** | Service role key (bypasses RLS) |

---

## Security Notes

- **Never commit `.env` to git** (already in `.gitignore`)
- **Service role key** has admin access — keep it secret
- **Use Cloud Run Secrets** for production deployment
- **Dockerfile runs as non-root user** in production (security best practice)

---

**Status:** ✅ Docker setup ready
**Last Updated:** 2026-08-21
