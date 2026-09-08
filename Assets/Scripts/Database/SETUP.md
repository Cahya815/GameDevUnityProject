# Game Pemadam - Complete Setup Guide

> Panduan lengkap setup Game Pemadam dari development lokal hingga production deployment.

## 📑 Table of Contents

1. [Project Overview](#project-overview)
2. [Prerequisites](#prerequisites)
3. [Backend Setup](#backend-setup)
4. [Database Setup (Supabase)](#database-setup)
5. [Frontend Setup (Unity)](#frontend-setup)
6. [Environment Configuration](#environment-configuration)
7. [Local Testing](#local-testing)
8. [Deployment](#deployment)
9. [API Endpoints](#api-endpoints)
10. [Troubleshooting](#troubleshooting)

---

## Project Overview

**Game Pemadam** adalah 3D isometric firefighting game dengan:
- **Backend**: Node.js/Express + Supabase
- **Frontend**: Unity (C#)
- **Authentication**: JWT tokens
- **Security**: AES-256-GCM encryption
- **Deployment**: Google Cloud Run
- **Database**: Supabase PostgreSQL

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Unity Client                          │
│              (3D Game + Login/Register UI)                   │
└─────────────────────────────────────┬───────────────────────┘
                                      │
                    JWT Token + AES Encrypted Data
                                      │
┌─────────────────────────────────────▼───────────────────────┐
│                   Express.js Backend                         │
│          (Cloud Run Container / Localhost:3000)              │
│                                                              │
│  Routes:                                                     │
│  ├─ POST /auth/register   (Register user)                   │
│  ├─ POST /auth/login      (Login user)                      │
│  ├─ GET  /auth/me         (Get profile)                     │
│  ├─ POST /v1/saves        (Save game data)                  │
│  ├─ GET  /v1/saves        (Retrieve game logs)              │
│  └─ GET  /v1/leaderboard/top (Global leaderboard)          │
└─────────────────────────────────────┬───────────────────────┘
                                      │
                          PostgreSQL Queries
                                      │
┌─────────────────────────────────────▼───────────────────────┐
│                    Supabase Database                         │
│                                                              │
│  Tables:                                                     │
│  ├─ users (id, email, username, password_hash, ...)         │
│  └─ game_logs (id, player_name, fires_extinguished, ...)    │
└──────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| Node.js | 16+ | Backend runtime |
| npm | 8+ | Package manager |
| Git | Latest | Version control |
| Unity | 2022.3+ | Game engine |
| Google Cloud SDK | Latest | Cloud Run deployment |
| Supabase Account | Free tier OK | Database hosting |

### Command to Check Versions

```bash
node --version
npm --version
git --version
```

### Accounts Needed

- **Supabase** - https://supabase.com (free tier)
- **Google Cloud Project** - https://console.cloud.google.com
- **GitHub** - For code repository (optional but recommended)

---

## Backend Setup

### Step 1: Navigate to Backend Directory

```bash
cd "Scripts/Database/backend"
```

### Step 2: Install Dependencies

```bash
npm install
```

This installs:
- `express` - Web framework
- `@supabase/supabase-js` - Database client
- `jsonwebtoken` - JWT authentication
- `bcryptjs` - Password hashing
- `dotenv` - Environment variables

### Step 3: Create `.env` File

Create file: `Scripts/Database/backend/.env`

```dotenv
# Server
PORT=3000
NODE_ENV=development

# Supabase
SUPABASE_URL=https://YOUR_PROJECT.supabase.co
SUPABASE_SERVICE_ROLE_KEY=your_service_role_key_here

# JWT Authentication
JWT_SECRET=your_jwt_secret_min_32_chars_here
JWT_EXPIRES_IN=7d

# AES Encryption
ENCRYPTION_SECRET=your_encryption_secret_min_32_chars_here
```

**Generate strong secrets:**

```bash
# Generate JWT_SECRET
node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"

# Generate ENCRYPTION_SECRET
node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
```

### Step 4: Verify Installation

```bash
npm run dev
```

Expected output:
```
🚀 Server running on port 3000 (development)
```

---

## Database Setup

### Step 1: Create Supabase Account

1. Go to https://supabase.com
2. Click "Start your project"
3. Sign in with GitHub/Google/Email
4. Create new organization
5. Create new project (name: `game-pemadam`, region: closest to you)

### Step 2: Get Credentials

In Supabase Dashboard:
1. **Project Settings** → **API**
   - Copy `Project URL` → `SUPABASE_URL`
   - Copy `Service Role Key` → `SUPABASE_SERVICE_ROLE_KEY`

### Step 3: Create Users Table

In Supabase Dashboard → **SQL Editor** → **New Query**, run:

```sql
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  email TEXT UNIQUE NOT NULL,
  username TEXT UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  last_login TIMESTAMP WITH TIME ZONE
);

-- Enable Row Level Security
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE users FORCE ROW LEVEL SECURITY;

-- Create policy for service role
CREATE POLICY "Service role can do anything" ON users
  FOR ALL USING (auth.role() = 'service_role');
```

### Step 4: Verify game_logs Table

Backend expects `game_logs` table (created during app usage). Structure:

```sql
-- Already exists, but here's the schema:
CREATE TABLE IF NOT EXISTS game_logs (
  id BIGSERIAL PRIMARY KEY,
  player_name TEXT NOT NULL,
  fires_extinguished INTEGER NOT NULL,
  duration INTEGER NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

---

## Frontend Setup (Unity)

### Step 1: Open Unity Project

```
D:\AREAS\3D\game\GamePemadam\Isometric\My project
```

### Step 2: Verify Required Scripts

These files should exist in `Assets/Scripts/Database/`:
- `AuthManager.cs` - Authentication logic
- `EC2Client.cs` - API client with JWT support
- `LoginUI.cs` - Login/Register UI
- `DTOs.cs` - Data transfer objects
- `GameDataManager.cs` - Game save/load
- `LeaderboardUI.cs` - Leaderboard display
- `SupabaseProvider.cs` - Database integration

### Step 3: Create Login Scene

1. **Create new scene**: `Assets/Scenes/LoginScene.unity`
2. **Add Canvas**:
   - Right-click in Hierarchy
   - UI → Canvas
3. **Add UI Elements**:
   - Email Input (InputField)
   - Username Input (InputField)
   - Password Input (InputField)
   - Login Button
   - Register Button
   - Offline Mode Button
   - Status Text
   - Loading Indicator (Panel)

### Step 4: Setup LoginUI Component

1. Create empty GameObject: `LoginManager`
2. Attach `LoginUI.cs` script
3. In Inspector, assign UI elements:
   - Email Input → emailInput
   - Username Input → usernameInput
   - Password Input → passwordInput
   - Login Button → loginButton
   - Register Button → registerButton
   - Offline Mode Button → offlineModeButton
   - Status Text → statusText
   - Loading Panel → loadingIndicator

### Step 5: Configure EC2Client

1. In scene, find `EC2Client` GameObject
2. In Inspector, set `API Base URL`:
   - **Development**: `http://localhost:3000`
   - **Production**: `https://your-cloud-run-url.run.app`

---

## Environment Configuration

### Backend Environment Variables

| Variable | Required | Example |
|----------|----------|---------|
| PORT | No | 3000 |
| NODE_ENV | No | development |
| SUPABASE_URL | Yes | https://xyz.supabase.co |
| SUPABASE_SERVICE_ROLE_KEY | Yes | eyJhbGc... |
| JWT_SECRET | Yes | 64-char hex string |
| JWT_EXPIRES_IN | No | 7d |
| ENCRYPTION_SECRET | Yes | 64-char hex string |

### Unity PlayerPrefs Keys

Game stores these locally:
- `game_pemadam_jwt_token` - JWT authentication token
- `game_pemadam_user_id` - Current user UUID
- `game_pemadam_username` - Current username

Clear with:
```csharp
PlayerPrefs.DeleteAll();
PlayerPrefs.Save();
```

---

## Local Testing

### Test 1: Server Startup

```bash
cd Scripts/Database/backend
npm run dev
```

Expected: `🚀 Server running on port 3000 (development)`

### Test 2: Health Check

```bash
curl http://localhost:3000/health
```

Expected response:
```json
{ "status": "ok" }
```

### Test 3: User Registration

```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "Password123"
  }'
```

Expected response:
```json
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "test@example.com",
    "username": "testuser",
    "token": "eyJhbGciOiJIUzI1NiIs..."
  },
  "error": null
}
```

### Test 4: User Login

```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123"
  }'
```

Expected: JWT token returned

### Test 5: Save Game (Protected)

```bash
TOKEN="your_jwt_token_here"

curl -X POST http://localhost:3000/v1/saves \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "playerName": "testuser",
    "firesExtinguished": 5,
    "duration": 120
  }'
```

Expected: Game save created

### Test 6: Leaderboard

```bash
curl http://localhost:3000/v1/leaderboard/top?limit=10
```

Expected: Top 10 players

---

## Deployment

### Build & Deploy

```bash
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/game-pemadam-api

gcloud run deploy game-pemadam-api \
  --image gcr.io/YOUR_PROJECT_ID/game-pemadam-api \
  --region asia-southeast1 \
  --allow-unauthenticated \
  --set-env-vars SUPABASE_URL=...,JWT_SECRET=...,ENCRYPTION_SECRET=...
```

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | No | Register user |
| POST | `/auth/login` | No | Login user |
| GET | `/auth/me` | Yes | Get profile |
| POST | `/v1/saves` | Yes | Save game |
| GET | `/v1/saves` | No | Get logs |
| GET | `/v1/leaderboard/top` | No | Leaderboard |
| GET | `/health` | No | Health check |

Response format:
```json
{
  "success": true,
  "data": { /* ... */ },
  "error": null
}
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Cannot find module | `npm install` |
| SUPABASE_URL not set | Create `.env` with correct values |
| Invalid JWT token | Login again to get fresh token |
| Port 3000 in use | Kill process: `netstat -ano \| findstr :3000` |
| Username sudah terpakai | Try different username |

---

## Quick Start (TL;DR)

```bash
# Backend
cd Scripts/Database/backend
npm install
# Create .env with secrets
npm run dev

# Supabase
# Run users table SQL

# Unity
# Configure EC2Client BaseUrl
# Test Register → Login → Save

# Deploy
gcloud run deploy ...
```

---

**Version:** 1.0.0  
**Last Updated:** 2026-09-06
