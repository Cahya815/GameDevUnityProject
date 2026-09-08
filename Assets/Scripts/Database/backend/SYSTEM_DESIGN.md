# 🏛️ Game-Data API — Architecture & System Design Specification

## 1. Executive Summary
* **Project Name:** Game-Data API (Simplified)
* **Target Domain:** Backend Infrastructure for Simulation Games (Unity Client)
* **Objective:** Provide a clean, lightweight, and scalable Cloud Game Logging & Leaderboard REST API service using existing database schema (`game_logs`).
* **Tech Stack:**
  - **Client:** Unity Engine (C# Client / Unity Editor)
  - **Backend API:** Node.js (Express.js — Plain JSON REST API)
  - **Database:** Supabase (PostgreSQL with `game_logs` Schema)
  - **Containerization:** Docker & Docker Compose
  - **Local Infrastructure / Emulation:** Floci.io / Local Docker Runtime (`http://localhost:8080`)
  - **Transport Security:** HTTP (Local Development Edge Gateway)

---

## 2. System Architecture Flow

[ Unity Game Client (Editor / Local Build) ]│├─► (1. Serialize Mission / Game Log Result)├─► (2. Construct Plain JSON Payload)│▼ HTTP Request (Plain JSON over Localhost)┌─────────────────────────────────────────────────────────┐│         Local Docker / Floci.io Runtime Engine          ││         (Node.js API Container @ port 8080)             ││                                                         ││  ┌──────────────────────┐    ┌──────────────────────┐   ││  │ Express Router       │ ─► │ Input Validation     │   ││  └──────────────────────┘    └──────────────────────┘   ││                                         │               ││                              ┌──────────┴───────────┐   ││                              │ Database Controller  │   ││                              └──────────────────────┘   │└──────────────────────────────┬──────────────────────────┘│▼ Service Role Access (Supabase JS SDK)┌───────────────────────────┐│ Supabase Postgres Database││  └─► Table: game_logs     │└───────────────────────────┘
---

## 3. Threat Model & Security Architecture

| Security Layer | Threat / Vulnerability | Mitigation Strategy |
|---|---|---|
| **Transport Security** | Local Network Interception | Restricted to loopback interface (`localhost` / `127.0.0.1`). |
| **Secret Isolation** | Exposed Supabase Service Keys in Client Build | Hardcoded database keys removed from client; Supabase JS SDK calls proxy exclusively through Node.js using local `.env` Service Role Key. |
| **Input Injection** | JSON Corruption / SQL Injection | Strict payload parameter checks in Express route handlers and parameterized queries via Supabase SDK. |
| **Database Protection** | Direct Client Writes / Unauthorized Data Access | Row Level Security (RLS) enabled on Postgres tables; direct client queries blocked. |

---

## 4. Database Schema Specification (Supabase / Postgres)

### Active Schema: `game_logs` Table
```sql
CREATE TABLE public.game_logs (
    id BIGSERIAL PRIMARY KEY,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW() NOT NULL,
    player_name TEXT NOT NULL,
    fires_extinguished INT NOT NULL DEFAULT 0,
    duration INT NOT NULL DEFAULT 0
);

-- Enable RLS
ALTER TABLE public.game_logs ENABLE ROW LEVEL SECURITY;
-- Service Role Key bypasses RLS for Express Backend writes
5. API Endpoint SpecificationBase URL: http://localhost:8080/v1 — all endpoints accept and return Plain JSON.Method & PathAuthRequest BodySuccess ResponsePOST /v1/savesPublic{ "playerName": "Player1", "firesExtinguished": 5, "duration": 120 }200 { "success": true, "data": { "id": 52, "created_at": "..." } }GET /v1/savesPublicQuery Param: ?playerName=Player1200 { "success": true, "data": [ { ... } ] }GET /v1/leaderboard/topPublicQuery Param: ?limit=10200 { "success": true, "data": [ { ... } ] }GET /healthNoneNone200 { "status": "ok" }6. Project Directory Structure & Core Filesbackend/
├── src/
│   ├── config.js         # Environment variables & Supabase setup
│   ├── db/
│   │   └── supabase.js   # Plain CRUD database operations for game_logs
│   └── index.js          # Express server & API route definitions
├── Dockerfile            # Container build definition
├── docker-compose.yml    # Local multi-container development orchestrator
├── package.json          # Dependencies (Express & @supabase/supabase-js)
├── .env                  # Local environment configuration
└── README.md             # API documentation
Implementation Reference (src/db/supabase.js)JavaScriptimport { createClient } from '@supabase/supabase-js';
import { config } from '../config.js';

const supabase = createClient(config.SUPABASE_URL, config.SUPABASE_SERVICE_ROLE_KEY);

export async function saveGameLog(playerName, firesExtinguished, duration) {
  const { data, error } = await supabase
    .from('game_logs')
    .insert([
      {
        player_name: playerName || 'Anonymous',
        fires_extinguished: firesExtinguished || 0,
        duration: duration || 0
      }
    ])
    .select();

  if (error) throw error;
  return data[0];
}

export async function getLogsByPlayer(playerName) {
  const { data, error } = await supabase
    .from('game_logs')
    .select('*')
    .eq('player_name', playerName)
    .order('created_at', { ascending: false });

  if (error) throw error;
  return data;
}

export async function getTopLeaderboard(limit = 10) {
  const { data, error } = await supabase
    .from('game_logs')
    .select('*')
    .order('fires_extinguished', { ascending: false })
    .order('duration', { ascending: true })
    .limit(limit);

  if (error) throw error;
  return data;
}
7. Environment Variables (.env)Cuplikan kodePORT=8080
NODE_ENV=development
SUPABASE_URL=[https://asxsyvbnuxmhacxbqazb.supabase.co](https://asxsyvbnuxmhacxbqazb.supabase.co)
SUPABASE_SERVICE_ROLE_KEY=your_secret_service_role_key
8. Deployment Notes (Local Cloud Emulation / Floci.io)Local Runtime: Configured to run locally using Docker Desktop / Floci.io environment.Port Mapping: Exposes port 8080 (0.0.0.0:8080:8080).Client Targeting: Unity Editor calls http://localhost:8080/v1/... directly during development and offline testing.