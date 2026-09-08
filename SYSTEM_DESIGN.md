# 🏛️ Secure Game-Data API — System Design & Learning Reference

**Last Updated:** 2026-08-17  
**Status:** Phase 1 Planning (Backend Scaffold)  
**Owner:** Cahya815

---

## 0. Learning Process & Engineering Mindset

> **CRITICAL CONTEXT:** This document is NOT a "press enter and copy-paste" artifact. It's a **learning scaffold** built together with Claude Code. Read, understand, and validate each section before implementation.

### Why This Matters (Jangkar Realitas)

You're learning to be an **Engineer**, not a code-generator user. The difference:
- **Tukang (Coder):** Presses "enter" → copies output → ships
- **Engineer:** Understands **Why**, validates **What**, owns **How**

AI generates statistically-probable code. Probability ≠ correctness. Your job is the **human filter**.

### Safe AI Usage Strategy (Strategi "Dikit-dikit Build")

| Phase | Action | Your Role | AI Role |
|-------|--------|-----------|---------|
| 1. Design | Architect the system | Decide structure | Suggest alternatives |
| 2. Scaffold | Build one module | Validate logic | Write code |
| 3. Test | Run & break it | Debug root cause | Fix implementation |
| 4. Learn | Rewrite key parts manually | Build muscle memory | Explain reasoning |
| 5. Ship | Review everything | Final audit | Document patterns |

**Golden Rule:** If you can't explain 80% of the code after AI writes it, don't commit. Rewrite it manually until you own it.

### Commitment to This Approach
- **Before each phase:** Read the spec section
- **Ask "why" 3 times:** If AI suggests something, ask why until you understand the root reason
- **Manual validation:** Run code locally before pushing
- **Commit messages:** Explain *what* changed and *why*, not just *what*
- **Review code line-by-line:** Don't skip sections

---

## 1. Executive Summary

**Project:** Secure Game-Data API  
**Domain:** Backend infrastructure for anti-cheat Cloud-Save + Global Leaderboard  
**Goal:** Production-grade security preventing score manipulation, memory tampering, IL2CPP decompilation exploits  

### Tech Stack (Final Decision with Rationale)

| Component | Choice | Why | Alternative Considered |
|-----------|--------|-----|------------------------|
| **Backend Language** | Node.js (Express.js) | You've studied it; lightweight; great async/crypto support | Python/FastAPI (more secure by default, but less familiar) |
| **Database** | Supabase (PostgreSQL) | Already wired in Unity; free tier; RLS policies work well | MongoDB Atlas (less relational data overhead) |
| **Cloud Deploy** | Google Cloud Run | Zero-config containerization; free tier generous; auto-scale | Compute Engine (more control, more complex) |
| **Encryption** | AES-256-GCM | Industry standard; Node.js native `crypto` module | RSA (slower, unnecessary for symmetric data) |
| **MVP Scope** | Full Cloud Save + Leaderboard | Captures all game state, not just scores | Leaderboard only (misses replay-attack surface) |
| **Cost Model** | Free tier | Cloud Run (2M requests/month free) + Supabase (500MB free) | Paid from day 1 (not justified at MVP stage) |

### What Gets Built (Phase Breakdown)

```
Phase 1: Backend Scaffold (THIS DOCUMENT)
  ├─ Package.json + dependencies
  ├─ Express.js basic server (listen, health check)
  ├─ Environment variable setup
  ├─ TypeScript config
  ├─ Docker skeleton
  └─ YOU UNDERSTAND: dependencies, server startup flow, env loading

Phase 2: Core Security Layer
  ├─ AES-256-GCM crypto helpers (encrypt/decrypt)
  ├─ JWT verify middleware (Google OAuth token)
  ├─ Input validation schemas (Zod DTOs)
  ├─ Anti-cheat logic (score sanity checks)
  └─ YOU UNDERSTAND: how encryption works, JWT flow, validation benefits

Phase 3: API Endpoints
  ├─ POST /api/v1/save (cloud save)
  ├─ POST /api/v1/load (cloud load)
  ├─ POST /api/v1/leaderboard/submit
  ├─ GET /api/v1/leaderboard/top
  └─ YOU UNDERSTAND: request/response contracts, error handling

Phase 4: Database Setup
  ├─ Create Supabase tables (game_saves, leaderboard, mission_history)
  ├─ Enable RLS policies
  ├─ Service role configuration
  └─ YOU UNDERSTAND: SQL schema design, RLS concept

Phase 5: Unity Client Changes
  ├─ Replace SupabaseProvider.cs with ApiClient.cs
  ├─ Implement CryptoHelper.cs (AES-256 in C#)
  ├─ GameStateDTO serialization
  ├─ Remove hardcoded keys
  └─ YOU UNDERSTAND: client-server encryption handoff, removed attack surface

Phase 6: Deployment
  ├─ Docker container build
  ├─ Cloud Run deployment
  ├─ Environment secrets setup
  ├─ Load testing
  └─ YOU UNDERSTAND: containerization, cloud deployment pipeline
```

**You start here ↓**

---

## 2. System Architecture

### Data Flow Diagram

```
┌─────────────────────────────────────┐
│     Unity Game Client (C#)          │
│                                     │
│  1. Serialize GameState to JSON     │
│  2. Encrypt w/ AES-256-GCM          │
│  3. Add IV + Auth Tag               │
└─────────────┬───────────────────────┘
              │
              │ HTTPS POST
              │ Content-Type: application/json
              │ Authorization: Bearer <JWT>
              │ Payload: {payload, iv, auth_tag}
              │
▼─────────────────────────────────────┐
│  Google Cloud Run (Node.js API)     │
│                                     │
│  ┌────────────────────────────────┐ │
│  │ 1. Verify JWT (Google OAuth)   │ │
│  │    Check: exp, aud, iss        │ │
│  └────────────────────────────────┘ │
│                      │               │
│  ┌────────────────────┴─────────────┐│
│  │ 2. Decrypt AES-256-GCM Payload  ││
│  │    Verify auth_tag integrity    ││
│  └────────────────────┬─────────────┘│
│                      │                │
│  ┌────────────────────┴─────────────┐│
│  │ 3. Validate DTO Schema (Zod)     ││
│  │    - Type checking               ││
│  │    - Bounds (score > 0)          ││
│  │    - Duration sanity (1-3600s)   ││
│  └────────────────────┬─────────────┘│
│                      │                │
│  ┌────────────────────┴─────────────┐│
│  │ 4. Anti-Cheat Validation         ││
│  │    - Score ≈ fires * 10          ││
│  │    - Replay detection (5min)     ││
│  │    - Rate limit (10/hr)          ││
│  └────────────────────┬─────────────┘│
│                      │                │
│  ┌────────────────────┴─────────────┐│
│  │ 5. Upsert to Postgres            ││
│  │    (Service Role = RLS bypass)   ││
│  └────────────────────┬─────────────┘│
└────────────────────────┼──────────────┘
                         │
                ▼────────────────────┐
                │ Supabase Postgres  │
                │                    │
                │ game_saves table   │
                │ leaderboard table  │
                │ mission_history    │
                └────────────────────┘
```

### Why This Architecture (Threat Prevention)

| Decision | Threat It Prevents | How |
|----------|-------------------|-----|
| **Client-side AES encryption** | MITM / Packet sniffing | Even if HTTPS is intercepted, attacker sees only encrypted blob, not plaintext scores |
| **Server decrypts & validates** | Client memory tampering | Player can't modify score in RAM before POST; server re-validates from scratch |
| **Anti-cheat logic on backend** | Score injection | Client sends `{"score": 999999}` but server checks: fires=1 expected score ~10, rejects |
| **Service Role for DB writes** | PlayerPrefs token manipulation | User can't edit `access_token` to bypass RLS; backend owns all DB access |
| **JWT with expiry** | Replay attacks (reusing old requests) | Token expires in 1 hour; attacker can't replay old mission submissions indefinitely |
| **Rate limiting (10/hr)** | Brute force / DoS | Attacker can't spam submissions to find pattern in scoring logic |

---

## 3. Threat Model & Security Layers

### Identified Threats in Current Project

| Threat | Currently Exists | Severity | Mitigation in New System |
|--------|------------------|----------|------------------------|
| **Hardcoded API Keys** | ✅ YES: `SupabaseProvider.cs:10` | 🔴 CRITICAL | Move to backend env vars only; remove from Unity build |
| **Plaintext Token in PlayerPrefs** | ✅ YES: `DeepLinkManager.cs:52` | 🔴 CRITICAL | JWT stored in `PlayerPrefs` but validated server-side; expires quickly |
| **JSON Injection** | ✅ YES: String concat `"player_name\":\"" + playerName` | 🟠 HIGH | Use `JSON.stringify()` + Zod schema validation |
| **Score Manipulation** | ✅ YES: Client sends arbitrary `fires_extinguished` | 🔴 CRITICAL | Server-side sanity: score must match fires × 10 ± margin |
| **IL2CPP Decompilation** | ✅ YES: Keys embedded in build | 🔴 CRITICAL | Encrypt all secrets; load from backend only |
| **Replay Attack** | ⚠️ PARTIAL: No JWT expiry/nonce | 🟠 HIGH | JWT with `exp` claim + duplicate submission detection (5min window) |

### Security Controls Implemented

#### Layer 1: Transport Security
- **HTTPS only** (Cloud Run auto-enforces; no plaintext HTTP)
- **No hardcoded secrets in client build** (removed from Unity)
- **Service keys live only in backend env vars** (Supabase key never leaves Cloud Run)

#### Layer 2: Authentication
- **Google OAuth** (user identity verified by Google, not self-signed)
- **JWT with expiry** (short-lived: 1 hour; forces periodic re-auth)
- **Token refresh logic** (client gets new JWT before old expires)

#### Layer 3: Encryption
- **AES-256-GCM** (authenticated symmetric encryption; detects tampering)
- **Dynamic IV** (random per request; prevents pattern analysis)
- **Auth Tag** (cryptographic proof payload wasn't modified in transit)

#### Layer 4: Validation
- **Schema validation** (Zod; enforce types at runtime)
- **Bounds checking** (score > 0, duration 60–3600s)
- **Type enforcement** (no string `score`; must be number)

#### Layer 5: Anti-Cheat
- **Score sanity** (fires × 10 ± 20% = valid range)
- **Duration sanity** (mission should take 1–60 minutes)
- **Duplicate detection** (block re-submission of identical mission within 5 minutes)
- **Rate limiting** (max 10 submissions per user per hour)

---

## 4. Database Schema (Supabase PostgreSQL)

### Table: `game_saves` (Full State Backup)

```sql
CREATE TABLE public.game_saves (
    user_id UUID PRIMARY KEY REFERENCES auth.users(id),
    encrypted_state TEXT NOT NULL,      -- AES-256 encrypted JSON (base64)
    iv TEXT NOT NULL,                   -- Initialization vector (base64)
    auth_tag TEXT NOT NULL,             -- Authentication tag (base64)
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

ALTER TABLE public.game_saves ENABLE ROW LEVEL SECURITY;

-- Service role (backend) can read/write all rows
-- (RLS policy only applies to authenticated users, not service role)
```

### Table: `leaderboard` (Global Ranking)

```sql
CREATE TABLE public.leaderboard (
    user_id UUID PRIMARY KEY REFERENCES auth.users(id),
    player_name VARCHAR(100) NOT NULL,
    high_score INT NOT NULL DEFAULT 0,
    fires_extinguished INT NOT NULL DEFAULT 0,
    total_missions INT NOT NULL DEFAULT 0,
    total_duration_seconds FLOAT NOT NULL DEFAULT 0,
    average_fires_per_mission FLOAT GENERATED ALWAYS AS (
        CASE WHEN total_missions > 0 THEN fires_extinguished::FLOAT / total_missions ELSE 0 END
    ) STORED,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_leaderboard_score DESC ON public.leaderboard(high_score DESC);
CREATE INDEX idx_leaderboard_user_id ON public.leaderboard(user_id);

ALTER TABLE public.leaderboard ENABLE ROW LEVEL SECURITY;
```

### Table: `mission_history` (Anti-Replay Audit Log)

```sql
CREATE TABLE public.mission_history (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES auth.users(id),
    fires_extinguished INT NOT NULL,
    duration_seconds FLOAT NOT NULL,
    submitted_at TIMESTAMPTZ DEFAULT NOW(),
    ip_address INET,  -- For rate limiting analysis
    client_version VARCHAR(50)
);

CREATE INDEX idx_mission_history_user_time ON public.mission_history(user_id, submitted_at DESC);
```

**Why This Schema:**
- Encrypted state stored as TEXT (no plain columns = zero data leakage if DB is breached)
- `mission_history` logs all submissions for auditing; enables duplicate detection via timestamp
- Indexes on `user_id + submitted_at DESC` enable fast "last 5 minutes" queries for replay detection

---

## 5. API Endpoints Contract

### 1. `POST /api/v1/save` — Cloud Save

**Purpose:** Persist full game state (HQ level, money, crew, unlocks)

**Request Body:**
```json
{
  "player_id": "550e8400-e29b-41d4-a716-446655440000",
  "payload": "U2FsdGVkX1...",  // Base64: AES-256-GCM encrypted JSON
  "iv": "abc123xyz",            // Base64: IV used for encryption
  "auth_tag": "def456uvw"       // Base64: HMAC authentication tag
}
```

**Response (200 OK):**
```json
{
  "status": "success",
  "message": "Game state securely synchronized.",
  "timestamp": "2026-08-17T14:00:00Z"
}
```

**Error (400 Bad Request):**
```json
{
  "status": "error",
  "message": "Invalid payload format or failed decryption",
  "code": "DECRYPT_FAILED"
}
```

---

### 2. `POST /api/v1/load` — Cloud Load

**Purpose:** Retrieve encrypted game state for client to decrypt & restore

**Request Body:**
```json
{
  "player_id": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (200 OK):**
```json
{
  "status": "success",
  "payload": "U2FsdGVkX1...",
  "iv": "abc123xyz",
  "auth_tag": "def456uvw",
  "updated_at": "2026-08-17T14:00:00Z"
}
```

---

### 3. `POST /api/v1/leaderboard/submit` — Mission Result

**Purpose:** Submit mission score, update leaderboard if it's a new personal best

**Request Body:**
```json
{
  "player_id": "550e8400-e29b-41d4-a716-446655440000",
  "player_name": "ChiefFirefighter",
  "fires_extinguished": 25,
  "duration_seconds": 180.5
}
```

**Anti-Cheat Validation (Server-Side):**
- `fires_extinguished` must be 0–100 (reasonable max)
- `duration_seconds` must be 60–3600 (1 min to 1 hour)
- Expected score ≈ `fires_extinguished * 10` ± 20% margin
- No duplicate submission within last 5 minutes
- Max 10 submissions per user per hour

**Response (200 OK):**
```json
{
  "status": "success",
  "is_new_high_score": true,
  "current_rank": 14,
  "personal_best": 25,
  "timestamp": "2026-08-17T14:00:00Z"
}
```

---

### 4. `GET /api/v1/leaderboard/top` — Global Ranking

**Purpose:** Fetch top N players

**Query Params:**
- `limit`: 1–100 (default: 50)
- `offset`: pagination (default: 0)

**Response (200 OK):**
```json
{
  "status": "success",
  "data": [
    {
      "rank": 1,
      "player_name": "ChiefFirefighter",
      "high_score": 25000,
      "total_missions": 120,
      "average_fires_per_mission": 208.3
    },
    {
      "rank": 2,
      "player_name": "RescueHero",
      "high_score": 21300,
      "total_missions": 95,
      "average_fires_per_mission": 224.2
    }
  ],
  "total_count": 5432,
  "timestamp": "2026-08-17T14:00:00Z"
}
```

---

### 5. `GET /health` — Liveness Probe

**Purpose:** Cloud Run health check for auto-scaling & monitoring

**Response (200 OK):**
```json
{
  "status": "ok",
  "uptime": 3600.5,
  "database": "connected"
}
```

---

## 6. Environment Variables (`.env` Template)

```env
# Server
PORT=8080
NODE_ENV=production
LOG_LEVEL=info

# Google OAuth
GOOGLE_CLIENT_ID=<from Google Cloud Console>

# Supabase (Service Role = backend-only access)
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_SERVICE_ROLE_KEY=<secret-key-never-share>

# AES-256 Encryption
AES_SECRET_KEY=<32-byte-hex-string>  # Generate: node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"

# Rate Limiting
RATE_LIMIT_WINDOW_MS=3600000  # 1 hour
RATE_LIMIT_MAX_REQUESTS=10

# Cloud Run
CLOUD_RUN_REGION=us-central1
```

**🚨 SECURITY NOTE:** Never commit `.env` to git. Use Cloud Run Secrets Manager instead.

---

## 7. File Structure (Backend Repository)

```
game-api/
├── src/
│   ├── main.ts              # Express.js server entry
│   ├── config.ts            # Environment loading + validation
│   ├── middleware/
│   │   ├── auth.ts          # JWT verification
│   │   ├── validation.ts    # Schema validation (Zod)
│   │   ├── errorHandler.ts  # Global error middleware
│   │   └── rateLimit.ts     # Rate limiting
│   ├── crypto/
│   │   ├── aes.ts           # AES-256-GCM encrypt/decrypt
│   │   └── jwt.ts           # JWT issue/verify
│   ├── routes/
│   │   ├── save.ts          # POST /api/v1/save
│   │   ├── load.ts          # POST /api/v1/load
│   │   ├── leaderboard.ts   # /api/v1/leaderboard/*
│   │   └── health.ts        # GET /health
│   ├── models/
│   │   ├── GameState.ts     # TypeScript interface (game state shape)
│   │   ├── MissionResult.ts # TypeScript interface (mission data)
│   │   └── LeaderboardEntry.ts
│   ├── services/
│   │   ├── supabase.ts      # Supabase client + queries
│   │   ├── antiCheat.ts     # Score validation logic
│   │   └── leaderboard.ts   # Ranking calculations
│   └── utils/
│       ├── logger.ts
│       └── validation.ts
├── migrations/              # SQL setup scripts
│   ├── 001_init_schema.sql
│   └── 002_indexes.sql
├── tests/
│   ├── crypto.test.ts
│   ├── antiCheat.test.ts
│   └── api.test.ts
├── Dockerfile
├── docker-compose.yml       # Local dev
├── package.json
├── tsconfig.json
├── .env.example
└── README.md
```

---

## 8. Implementation Checklist (Incremental)

### Phase 1: Scaffold ✅ (START HERE)

- [ ] Create Node.js + Express.js project
- [ ] `package.json` with dependencies
  - `express` (server framework)
  - `zod` (schema validation at runtime)
  - `@supabase/supabase-js` (Postgres client)
  - `jsonwebtoken` (JWT verify)
  - `dotenv` (env variable loading)
  - `typescript` (type safety)
  - `ts-node` (run TS directly)
- [ ] Basic Express server listening on `:8080`
- [ ] `/health` endpoint working (returns `{status: "ok"}`)
- [ ] Environment variable loading (no crashes if `.env` missing)
- [ ] **VALIDATE:** `npm start` → logs "Server listening on :8080"
- [ ] **YOU UNDERSTAND:** What each dependency does, why we chose it

### Phase 2: Security Foundations

- [ ] AES-256-GCM crypto helper
  - `encrypt(plaintext, key)` → `{ ciphertext, iv, authTag }`
  - `decrypt(ciphertext, iv, authTag, key)` → plaintext
- [ ] JWT verify middleware
- [ ] Input validation schemas (Zod)
- [ ] **VALIDATE:** Unit tests pass for encrypt/decrypt round-trip
- [ ] **YOU UNDERSTAND:** How AES-GCM works, why IV is random, what Auth Tag does

### Phase 3: API Endpoints

- [ ] `POST /api/v1/save` (decrypt, validate, upsert to DB)
- [ ] `POST /api/v1/load` (fetch encrypted state)
- [ ] `POST /api/v1/leaderboard/submit` (anti-cheat checks)
- [ ] `GET /api/v1/leaderboard/top` (top 50)
- [ ] **VALIDATE:** Postman tests for each endpoint
- [ ] **YOU UNDERSTAND:** Request/response contracts, error handling patterns

### Phase 4: Database

- [ ] Create Supabase tables (game_saves, leaderboard, mission_history)
- [ ] Service role SQL scripts
- [ ] **VALIDATE:** Data persists after INSERT/SELECT
- [ ] **YOU UNDERSTAND:** Why service role exists, what RLS does

### Phase 5: Docker & Cloud Run

- [ ] `Dockerfile` (Node.js slim image)
- [ ] Build image locally: `docker build -t game-api .`
- [ ] Deploy to Cloud Run
- [ ] Test production endpoint
- [ ] **VALIDATE:** `curl https://<cloud-run-url>/health`
- [ ] **YOU UNDERSTAND:** What Docker does, how Cloud Run loads env vars

### Phase 6: Unity Client Integration

- [ ] New `ApiClient.cs` (replaces SupabaseProvider)
- [ ] `CryptoHelper.cs` (AES-256 in C#)
- [ ] Update `GameDataManager.cs` to use `ApiClient`
- [ ] **VALIDATE:** Mission saves to backend; load restores state
- [ ] **YOU UNDERSTAND:** How C# crypto mirrors Node.js, why secrets are gone

---

## 9. Anti-Patterns to Avoid

| ❌ DON'T | ✅ DO | Why |
|--------|------|-----|
| Hardcode secrets in code | Load from `process.env` only | Secrets exposed in git history + build artifacts |
| String concat JSON | Use `JSON.stringify()` + Zod validation | Injection attacks; unescaped quotes break parsing |
| Trust client-provided score | Validate against server logic | Score manipulation; cheating trivial |
| Store plaintext tokens | Encrypt at rest, validate on server | IL2CPP decompilation; token theft |
| Skip rate limiting | Max N requests per user/hour | DoS attacks; infinite submission spam |
| Return detailed error messages | Log internally, return generic message | Information leakage (e.g., "user not found" reveals who's registered) |
| Use `any` in TypeScript | Strict types everywhere | Type errors missed at compile-time; runtime surprises |
| Async/await without error handling | Try-catch or `.catch()` | Unhandled promise rejections crash server |
| Store IV + key in same place | IV stored separately (OK), key in env vars | IV doesn't need to be secret; key does |
| Encrypt without authentication | Use GCM (encrypt + authenticate) | Decryption succeeds but data was modified in transit |

---

## 10. Key Learning Decisions (Why These Choices)

### Why Node.js + Express (vs. Python/FastAPI)?
- You've studied Node.js already → faster ramp-up
- Lightweight; good for microservices
- Native async/await (better than callback hell)
- Great crypto support (`crypto` module is built-in)
- Fast iteration cycle

### Why Supabase (vs. MongoDB)?
- Already wired in your game (Google Auth integrated)
- PostgreSQL = better for relational data (leaderboard queries with rankings)
- RLS policies = security enforced at DB level (not just application code)
- Free tier generous enough for MVP development

### Why Cloud Run (vs. Compute Engine)?
- Containerized = same code runs locally + production
- Zero-config = push Docker image, it scales automatically
- Free tier: 2M requests/month (plenty for dev/testing)
- Cheap scaling: ~$0.10 per 1M requests after free tier
- **You don't have to manage VMs** (reduces operational burden)

### Why AES-256-GCM (vs. RSA)?
- Symmetric encryption = fast (RSA would be 100x slower)
- GCM mode = encryption + authentication in one operation
- Native in Node.js `crypto` module = no extra dependencies
- Standard practice for application data encryption

### Why Zod for validation (vs. Joi/Yup)?
- TypeScript-native; infers types automatically
- Runtime validation with compile-time type safety
- Smaller bundle size
- Growing ecosystem

---

## 11. Deployment Checklist

### Before You Ship

```bash
# 1. All tests pass
npm run test

# 2. No hardcoded secrets in code
grep -r "sk_" src/       # Supabase key pattern
grep -r "GOOG" src/      # Google key pattern

# 3. Environment variables defined
cat .env.example | sort

# 4. Docker builds locally
docker build -t game-api .
docker run -p 8080:8080 game-api

# 5. `/health` responds
curl http://localhost:8080/health

# 6. Database connected
npm run test:db

# 7. All endpoints tested in Postman
# (or automated E2E tests pass)
```

### Deploy to Cloud Run

```bash
# 1. Push Docker image to Google Artifact Registry
gcloud builds submit --tag gcr.io/PROJECT_ID/game-api

# 2. Deploy from image
gcloud run deploy game-api \
  --image gcr.io/PROJECT_ID/game-api \
  --region us-central1 \
  --set-env-vars SUPABASE_URL=...,SUPABASE_SERVICE_ROLE_KEY=...

# 3. Test production
curl https://game-api-xxx.run.app/health
```

---

## 12. Future Phases (Not This Sprint)

- [ ] OAuth token refresh mechanism
- [ ] WebSocket for live leaderboard updates
- [ ] Admin panel for banning cheaters
- [ ] Analytics dashboard (revenue, DAU, engagement)
- [ ] Multi-region deployment (latency optimization)
- [ ] Database backups automation
- [ ] Metrics/monitoring (Datadog, New Relic)
- [ ] Load testing (k6, locust)

---

## 13. References & Resources

- [Node.js Crypto Docs](https://nodejs.org/api/crypto.html)
- [Supabase RLS Guide](https://supabase.com/docs/guides/auth/row-level-security)
- [Cloud Run Quickstart](https://cloud.google.com/run/docs/quickstarts/build-and-deploy)
- [AES-256-GCM Explanation](https://en.wikipedia.org/wiki/Galois/Counter_Mode)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [Express.js Best Practices](https://expressjs.com/en/advanced/best-practice-performance.html)

---

## 14. Sign-Off & Next Step

**This document is your north star.** Refer back to it before every phase. If something in implementation contradicts this design, **stop and update this document first**. Don't let code drift.

**Next Action:** 
1. ✅ You've reviewed this document
2. ⏭️ We create `package.json` + basic Express server
3. ⏭️ You run it locally, see `/health` respond
4. ⏭️ You understand every line before committing

**Questions to ask before proceeding (validate your understanding):**
- Can you explain why we encrypt on client, validate on server? (Why not just server-side encryption?)
- Can you draw the AES-256-GCM flow (IV → plaintext → ciphertext → auth tag)?
- How does rate limiting prevent abuse?
- Why is the service role key kept out of the Unity build?

If you can answer all four → ready for Phase 1 implementation. If not → re-read sections 2, 3, 10 until it clicks.

---

**Document Status:** Living reference. Updates required as implementation progresses. Sections will be marked ✅ as phases complete. Last modified: 2026-08-17
