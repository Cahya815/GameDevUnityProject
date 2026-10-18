# 🚒 Game Telemetry, Mission Save & Leaderboard Service (Cloud-Native REST API)

![Node.js](https://img.shields.io/badge/Node.js-Express_4-339933?style=flat&logo=nodedotjs&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Supabase-4169E1?style=flat&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Multi--Stage-2496ED?style=flat&logo=docker&logoColor=white)
![Google Cloud Run](https://img.shields.io/badge/Google_Cloud-Cloud_Run-4285F4?style=flat&logo=googlecloud&logoColor=white)
![JWT](https://img.shields.io/badge/Security-Stateless_JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white)

> **Project Status:** Production-Ready / Active Development  
> **Architecture:** Node.js (Express) REST API + Supabase (PostgreSQL) + Docker Multi-Stage + Google Cloud Run (with Local Floci GCP Emulation) + Unity 3D Client (C#).

---

## 1. Executive Summary

A production-ready, containerized **Backend REST API service** engineered for real-time game telemetry, stateless player authentication, mission state persistence, and global leaderboard aggregation. 

Built as the cloud backbone for an **Isometric 3D Disaster & Firefighting Simulation Game**, this service handles concurrent HTTP REST payloads from Unity C# clients, enforces strict authentication and payload validation, and is architected for zero-downtime serverless deployment on **Google Cloud Run**.

### 🔐 Security & Architectural Highlights
1. **Stateless JWT Authentication:** Protected routes (`/auth/me`, `POST /v1/saves`) enforce Bearer Token verification middleware to prevent unauthorized telemetry injection or score spoofing.
2. **Cryptographic Data Integrity:** Passwords are salted and hashed via `bcrypt` (enforcing a minimum 8-character policy) alongside strict request payload validation before database transactions.
3. **Multi-Stage Containerization:** Configured with Docker & Docker Compose supporting standardized environment injection (`PORT`, `JWT_SECRET`, `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY`) and dual port mapping (`3000` for local client debugging, `8080` for GCP Cloud Run runtime).
4. **Container Orchestration Readiness:** Exposes a lightweight `/health` probe endpoint reporting server uptime and timestamps for Kubernetes / Cloud Run liveness and readiness checks.
5. **Cost-Zero Cloud Emulation Lab:** Integrated with **Floci GCP Emulator** (`floci-gcp` & `floci-ui`) to simulate serverless Cloud Run deployments and container lifecycle orchestration locally.

---

## 2. System Architecture & Data Flow

| Layer | Technology | Implementation Details |
|---|---|---|
| **Backend API** | Node.js (Express 4) | RESTful routing, JWT Bearer Auth middleware, `bcrypt` hashing, CORS policy |
| **Database** | Supabase (PostgreSQL) | Managed PostgreSQL storing `users` (credentials/auth) & `game_logs` (telemetry) |
| **Containerization** | Docker & Compose | Multi-stage builds (`development` & `production` targets) |
| **Cloud Target** | Google Cloud Run (GCP) | Serverless container execution listening on standardized port `8080` |
| **Cloud Emulator** | Floci (`floci-gcp`) | Local GCP Cloud Run control plane simulation (Port `4588` & UI Port `4500`) |
| **Client Consumer** | Unity Engine (C#) | Custom `EC2Client.cs` & `AuthManager.cs` consuming REST JSON endpoints |

### Architecture Diagram

```text
[ Unity 3D Game Client (C#) ]
       │
       ▼ (HTTPS REST JSON via EC2Client.cs / Bearer JWT)
┌────────────────────────────────────────────────────────┐
│           Docker Container / Google Cloud Run          │
│              (Node.js Express REST Server)             │
│                                                        │
│  ├── GET  /health              (Liveness Probe)        │
│  ├── POST /auth/register       (bcrypt Password Hash)  │
│  ├── POST /auth/login          (Stateless JWT Issue)   │
│  ├── GET  /auth/me             (JWT Auth Middleware)   │
│  ├── POST /v1/saves            (Protected Telemetry)   │
│  └── GET  /v1/leaderboard/top  (Query Aggregation)     │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼ (Supabase JS Client / Service Role Key)
┌────────────────────────────────────────────────────────┐
│             Supabase Cloud (PostgreSQL)                │
│                                                        │
│  ├── Table: `users`      (UUID, Email, Password Hash)  │
│  └── Table: `game_logs`  (Mission Scores & Duration)   │
└────────────────────────────────────────────────────────┘

```

---

## 3. REST API Reference & Contract Specifications

**Base URL:** `http://localhost:3000` (Local Dev) or `https://<service-hash>.a.run.app` (GCP Cloud Run)

### Endpoint Summary

| Method | Endpoint | Auth Required | Query / Payload | Description |
| --- | --- | --- | --- | --- |
| `GET` | `/health` | Public | None | Container readiness/liveness probe & server uptime |
| `POST` | `/auth/register` | Public | `{"email", "username", "password"}` | Register new player account (password min. 8 chars) |
| `POST` | `/auth/login` | Public | `{"email", "password"}` | Authenticate credentials & issue signed JWT |
| `GET` | `/auth/me` | **Bearer Token** | None | Retrieve authenticated player profile metadata |
| `POST` | `/v1/saves` | **Bearer Token** | `{"playerName", "firesExtinguished", "duration"}` | Commit completed mission telemetry to database |
| `GET` | `/v1/saves` | Public | `?limit=100` | Fetch chronological mission completion logs |
| `GET` | `/v1/leaderboard/top` | Public | `?limit=10` | Retrieve top-performing players sorted by score |

---

### Detailed Endpoint Payloads

#### 1. Container Health Check

* **Endpoint:** `GET /health`
* **Auth:** None
* **Response (`200 OK`):**

```json
{
  "status": "ok",
  "uptime": 120.45,
  "timestamp": "2026-10-18T08:00:00.000Z"
}

```

#### 2. Player Registration

* **Endpoint:** `POST /auth/register`
* **Auth:** None
* **Request Body:**

```json
{
  "email": "player@example.com",
  "username": "firefighter_one",
  "password": "securepassword123"
}

```

* **Response (`201 Created`):**

```json
{
  "message": "User registered successfully",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}

```

#### 3. Player Authentication (Login)

* **Endpoint:** `POST /auth/login`
* **Auth:** None
* **Request Body:**

```json
{
  "email": "player@example.com",
  "password": "securepassword123"
}

```

* **Response (`200 OK`):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "username": "firefighter_one",
    "email": "player@example.com"
  }
}

```

#### 4. Retrieve Authenticated Profile

* **Endpoint:** `GET /auth/me`
* **Auth:** Bearer Token (`Authorization: Bearer <JWT_TOKEN>`)
* **Response (`200 OK`):**

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "username": "firefighter_one",
  "email": "player@example.com",
  "last_login": "2026-10-18T08:15:30.000Z"
}

```

#### 5. Save Game Mission Telemetry

* **Endpoint:** `POST /v1/saves`
* **Auth:** Bearer Token (`Authorization: Bearer <JWT_TOKEN>`)
* **Request Body:**

```json
{
  "playerName": "firefighter_one",
  "firesExtinguished": 42,
  "duration": 360
}

```

* **Response (`201 Created`):**

```json
{
  "message": "Mission log saved successfully",
  "data": {
    "id": 104,
    "player_name": "firefighter_one",
    "fires_extinguished": 42,
    "duration": 360,
    "created_at": "2026-10-18T08:21:00.000Z"
  }
}

```

#### 6. Global Leaderboard & Mission History

* **Endpoints:** `GET /v1/leaderboard/top?limit=10` | `GET /v1/saves?limit=100`
* **Auth:** None
* **Response (`200 OK`):**

```json
[
  {
    "id": 104,
    "player_name": "firefighter_one",
    "fires_extinguished": 42,
    "duration": 360,
    "created_at": "2026-10-18T08:21:00.000Z"
  }
]

```

---

## 4. Database Schema (PostgreSQL / Supabase)

### Table: `public.users`

Stores player identity, unique constraints, and salted `bcrypt` password hashes.

```sql
CREATE TABLE public.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW() NOT NULL,
    email TEXT UNIQUE NOT NULL,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    last_login TIMESTAMP WITH TIME ZONE
);

```

### Table: `public.game_logs`

Stores immutable telemetry records for completed game sessions and leaderboard aggregation.

```sql
CREATE TABLE public.game_logs (
    id BIGSERIAL PRIMARY KEY,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW() NOT NULL,
    player_name TEXT NOT NULL,
    fires_extinguished INT NOT NULL DEFAULT 0,
    duration INT NOT NULL DEFAULT 0
);

```

---

## 5. Infrastructure, Docker & Cloud Deployment

### A. Local Container Orchestration (Docker Compose)

Navigate to the backend service directory:

```bash
cd Assets/Scripts/Database/backend

```

Build and spin up the containerized service in detached mode:

```bash
docker compose up -d --build

```

* **Port Mapping:**
* `3000`: Exposed for local Unity client development (`EC2Client.cs`).
* `8080`: Mapped to mirror Google Cloud Run's default container ingress port.


* **Verify Container Readiness:**

```bash
curl http://localhost:3000/health
# Expected output: {"status":"ok","uptime":...}

```

*(Alternative)* **Native Node.js Execution:**

```bash
npm install
npm run dev

```

---

### B. Production Deployment (Google Cloud Run)

1. **Build & Push Container Image to Google Artifact/Container Registry:**
```bash
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/game-api

```


2. **Deploy Stateless Service to Cloud Run:**
```bash
gcloud run deploy game-api \
  --image gcr.io/YOUR_PROJECT_ID/game-api \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars SUPABASE_URL=your-supabase-url,SUPABASE_SERVICE_ROLE_KEY=your-key,JWT_SECRET=your-secret

```


3. **Client Pointing:** Copy the provisioned HTTPS Cloud Run URL (e.g., `https://game-api-xyz.a.run.app`) into the `apiBaseUrl` serialized field on the `EC2Client` GameObject in Unity.

---

### C. 🔬 Local Serverless Emulation Lab (Floci GCP)

To validate Cloud Run deployment manifests and container port bindings locally without incurring cloud billing:

1. **Start the Floci GCP Control Plane Emulator:**
```bash
floci gcp start

```


2. **Provision the Containerized Service onto Local Cloud Run:**
```bash
curl -X POST "http://localhost:4588/v2/projects/floci-local/locations/us-central1/services?serviceId=game-api" \
  -H "Content-Type: application/json" \
  -d '{"template":{"containers":[{"image":"backend-api:latest","ports":[{"containerPort":8080}]}]}}'

```


3. **Observability & Lifecycle Monitoring:**
Access the Floci Web Console at `http://localhost:4500/console/gcp` to inspect service health, container logs, and `SUCCEEDED` deployment states.

---

## 6. Client Integration Overview (Unity 3D Simulation)

While the core focus of this repository's infrastructure is the backend & cloud pipeline, the API serves an active **Isometric 3D Disaster Management Game** built with Unity (C#):

* **Asynchronous REST Integration (`EC2Client.cs` & `AuthManager.cs`):** Handles non-blocking HTTP requests, Bearer token injection, session persistence, and fallback state management (`GameDataManager.cs`).
* **Dynamic Telemetry Generation:** Gameplay systems—including dynamic fire propagation (`Flammable.cs`), autonomous NavMesh agent dispatch (`FireTruck.cs`, `FirefighterCrew.cs`, `DisasterUnit.cs`), municipal budget calculation (`EconomyManager.cs`), and procedural difficulty scaling (`LevelDirector.cs`)—aggregate session metrics (`firesExtinguished` and survival `duration`) and commit them to `/v1/saves` upon mission completion.

---

## 7. Repository Directory Structure

```text
My project/
├── Assets/
│   ├── Scripts/
│   │   ├── Database/                  # 🌟 BACKEND & CLOUD INTEGRATION CORE
│   │   │   ├── backend/               # Node.js Express REST API Service
│   │   │   │   ├── Dockerfile         # Multi-stage container build definition
│   │   │   │   ├── docker-compose.yml # Service orchestration & port mapping (3000/8080)
│   │   │   │   ├── .env.example       # Environment variable template (Supabase & JWT)
│   │   │   │   └── src/               # Express app, JWT middleware, routes, & DB client
│   │   │   ├── AuthManager.cs         # Client-side JWT session & auth state manager
│   │   │   ├── EC2Client.cs           # Unity HTTP REST client for backend communication
│   │   │   ├── GameDataManager.cs     # Data provider abstraction (Cloud vs Local)
│   │   │   └── ModeSelectionUI.cs     # Authentication UI controller (Login/Register)
│   │   │
│   │   ├── FireTruck.cs               # Fleet water tank & NavMesh dispatch logic
│   │   ├── FirefighterCrew.cs         # Field personnel AI agent
│   │   ├── DisasterUnit.cs            # Post-fire debris clearance unit
│   │   ├── Flammable.cs               # Physics.OverlapSphere fire spread state machine
│   │   ├── EconomyManager.cs          # City treasury, rewards & penalty system
│   │   ├── LevelDirector.cs           # Procedural disaster pacing controller
│   │   ├── UnitIdentity.cs            # Engine degradation & crew upgrade metrics
│   │   ├── UnitManager.cs             # Fleet selection & global recall controller
│   │   ├── HQ/                        # Command center upgrades (Levels 1-4)
│   │   └── Tutorial/                  # Player onboarding state flow
└── README.md                          # System architecture & API documentation



```
