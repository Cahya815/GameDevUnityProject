# 🚒 Game Pemadam Kebakaran (Isometric 3D Simulation)

> **Status Proyek:** Beta / Active Development  
> **Arsitektur:** Unity (C#) Client + Node.js (Express) REST API + Supabase (PostgreSQL) + Docker & Google Cloud Run (Floci Emulator).

---

## 1. Ringkasan Proyek

**Game Pemadam Kebakaran** adalah game simulasi strategi & manajemen bencana berperspektif isometrik (3D). Pemain bertindak sebagai **Komandan Markas (Headquarters / HQ)** yang bertanggung jawab atas penanggulangan bencana kota, pemadaman kebakaran rumah warga & hutan, pembersihan puing reruntuhan, pemeliharaan armada kendaraan, serta pengelolaan keuangan kota.

Proyek ini dirancang sebagai arsitektur **Full-Stack Game-Cloud Integration**:
* **Frontend:** Unity 3D Engine (C#).
* **Backend:** Node.js (Express.js REST API).
* **Database:** Supabase Cloud (PostgreSQL).
* **Infrastruktur & Cloud:** Docker multi-stage, Google Cloud Run architecture, serta emulator cloud lokal **Floci (GCP & AWS)**.

---

## 2. Tech Stack & Arsitektur Sistem

| Komponen | Teknologi | Keterangan |
|---|---|---|
| **Game Engine** | Unity (C#) | NavMesh Navigation, Isometric Camera, Event State Machine |
| **Backend API** | Node.js (Express 4) | REST API, JWT Authentication, bcrypt hashing, CORS |
| **Database** | Supabase (PostgreSQL) | Tabel `users` (auth) & `game_logs` (riwayat misi & leaderboard) |
| **Containerization** | Docker & Docker Compose | Multi-stage build (`development` & `production`) |
| **Cloud Target** | Google Cloud Run (GCP) | Serverless container deployment (Port 8080) |
| **Cloud Emulator** | Floci (`floci-gcp` & `floci-ui`) | Simulasi lokal Cloud Run (Port 4588 & Web UI Port 4500) |

### Diagram Alur Data (Data Flow):

```text
[ Pemain di Game Unity (C#) ]
       │
       ▼ (HTTP REST JSON via EC2Client.cs)
┌────────────────────────────────────────────────────────┐
│             Docker / Google Cloud Run                  │
│             (Node.js Express Server)                   │
│                                                        │
│   ├── /auth/register & /auth/login (JWT & bcrypt)      │
│   ├── /v1/saves (Penyimpanan Misi Pemadam)             │
│   └── /v1/leaderboard/top (Papan Peringkat Global)     │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼ (Supabase JS Client / Service Role)
┌────────────────────────────────────────────────────────┐
│             Supabase Cloud (PostgreSQL)                │
│                                                        │
│   ├── Tabel `users`      (Data Akun & Hash Password)   │
│   └── Tabel `game_logs`  (Skor Api Padam & Durasi)     │
└────────────────────────────────────────────────────────┘
```

---

## 3. Cara Menjalankan Proyek (Quick Start)

### A. Menjalankan Backend API

Buka terminal dan masuk ke direktori backend:
```bash
cd Assets/Scripts/Database/backend
```

Pilih salah satu metode yang sesuai dengan lingkungan kerja Anda:

#### Opsi 1: Menggunakan Docker Compose (Direkomendasikan)
Pastikan Docker Desktop aktif, lalu jalankan:
```bash
docker compose up -d --build
```
* **Port yang Dibuka:**
  * `3000`: Digunakan oleh client game Unity.
  * `8080`: Standar environment Google Cloud Run.
* **Verifikasi:** Buka browser atau jalankan `curl http://localhost:3000/health` (menghasilkan `{"status":"ok"}`).

#### Opsi 2: Menjalankan Native (Tanpa Docker)
Jika ingin menjalankan langsung menggunakan Node.js di komputer lokal:
```bash
npm install
npm run dev
```
* Backend akan aktif langsung di `http://localhost:3000`.

---

### B. Menjalankan Game di Unity

1. Buka folder `My project` menggunakan **Unity Hub** (versi Unity 2022 LTS / yang setara).
2. Buka scene utama game.
3. Pastikan GameObject **`EC2Client`** pada Hierarchy terarah ke endpoint backend:
   * Default: `http://localhost:3000`
4. Tekan tombol **Play (▶️)** di Unity.
5. Alur Onboarding Game:
   ```text
   Login / Register Panel  ──►  Tutorial HQ Onboarding  ──►  Gameplay Darurat Aktif
   ```

---

## 4. Mekanik Inti Gameplay

### A. Armada Pemadam (`FireTruck.cs` & `FirefighterCrew.cs`)
* **Tangki Air:** Truk memiliki kapasitas air (`currentWater` / `maxWater`). Air berkurang saat menyemprot api dan terisi ulang secara otomatis saat berada di dekat **Markas HQ** atau **Hydrant**.
* **Deploy Kru Personel:** Truk parkir di jalan terdekat dari target kebakaran, lalu menurunkan kru personel damkar yang berjalan kaki menuju titik api via NavMesh.
* Setelah api padam atau air habis, kru otomatis kembali masuk ke dalam truk.

### B. Penanggulangan Bencana & Puing (`DisasterUnit.cs`)
* Kendaraan alat berat khusus untuk membersihkan puing reruntuhan rumah pasca kebakaran.
* Memberikan reward uang saat berhasil membersihkan puing rumah.
* **Aturan Khusus:** Tidak membersihkan pohon gosong, karena pohon di hutan memiliki mekanik regenerasi mandiri.

### C. Karakteristik Api & Lingkungan (`Flammable.cs`)
* Objek memiliki 3 status: `HouseStatus.Aman`, `Terbakar`, `Puing`.
* **Rumah Warga:** Ketahanan lebih tinggi, memadamkan rumah memberi reward ekonomi besar (+$50). Jika hancur menjadi puing, kas kota rugi (-$150).
* **Pohon Hutan:** Mudah terbakar & merembet lebih cepat, namun memiliki fitur **Regenerasi Otomatis** kembali hijau setelah beberapa saat menjadi pohon gosong.
* **Penyebaran Api:** Api merembet ke objek di sekitarnya (`Physics.OverlapSphere`) dengan variasi interval acak dinamis (`spreadVariance`).

### D. Markas (HQ), Pelatihan Kru, & Perawatan Mesin
* **Markas HQ (`HQController.cs`):** Dapat di-upgrade dari Level 1 hingga Level 4.
  * **HQ Level 1:** Pembelian unit pemadam dasar.
  * **HQ Level 2:** Membuka fitur Latihan Kru & membuka pembelian Disaster Unit ($510).
  * **HQ Level 3:** Membuka fitur Perawatan / Servis Mesin armada.
  * **HQ Level 4:** Pelatihan kru tanpa batas level.
* **Kondisi Mesin (`engineCondition`):** Mesin kendaraan mengalami keausan (`engineDegradationRate`) saat beroperasi. Jika menyentuh 0%, armada **mogok** dan wajib diservis di HQ.
* **Latihan Kru (`crewLevel`):** Meningkatkan kekuatan semprotan (`power`) dan kecepatan gerak unit.

### E. Manajemen Ekonomi & Level Director
* **Ekonomi Kota (`EconomyManager.cs`):** Saldo kas pemain bertambah dari bonus pemadaman api & pembersihan puing, serta berkurang untuk biaya beli unit ($200), servis mesin, latihan kru, dan penalti rumah hancur.
* **Level Director (`LevelDirector.cs`):** Mengatur frekuensi kemunculan api secara otomatis berdasarkan tingkat level kesulitan setelah masa tenggang (*grace period*) tutorial selesai.

---

## 5. Dokumentasi REST API Backend

Base URL: `http://localhost:3000` (atau port Cloud Run)

| Method | Endpoint | Auth | Request Body | Deskripsi |
|---|---|---|---|---|
| `GET` | `/health` | Public | None | Health check status server |
| `POST` | `/auth/register` | Public | `{"email", "username", "password"}` | Pendaftaran akun pemain baru (Password min. 8 karakter) |
| `POST` | `/auth/login` | Public | `{"email", "password"}` | Login akun pemain & mengembalikan JWT Token |
| `GET` | `/auth/me` | Bearer Token | None | Mengambil informasi profil akun yang sedang login |
| `POST` | `/v1/saves` | Bearer Token | `{"playerName", "firesExtinguished", "duration"}` | Menyimpan hasil misi permainan |
| `GET` | `/v1/saves` | Public | Query param: `?limit=100` | Mengambil seluruh log riwayat misi |
| `GET` | `/v1/leaderboard/top` | Public | Query param: `?limit=10` | Mengambil papan peringkat pemain terbaik |

---

## 6. Spesifikasi Skema Database (Supabase PostgreSQL)

### Tabel `users`
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

### Tabel `game_logs`
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

## 7. Kontrol Permainan (Keyboard & Mouse)

| Tombol | Aksi |
|---|---|
| `1` / `2` / `3` | Memilih unit armada (Firefighter Truck / Disaster Unit) |
| `0` | **Recall All Units:** Memerintahkan semua unit membatalkan tugas & kembali ke HQ |
| `Esc` | Melepas seleksi unit aktif (unit tetap melanjutkan tugas otomatisnya) |
| `Klik Kiri (HQ)` | Membuka panel upgrade & menu Markas HQ |
| `Klik Kiri (Tanah/Objek)` | Memerintahkan unit yang dipilih bergerak atau menuju target bencana |
| `Klik Kanan` | Alternatif navigasi pergerakan unit terpilih |

---

## 8. Panduan Deployment ke Production (Google Cloud Run Asli)

Jika sudah siap merilis backend ke Google Cloud Platform resmi:

1. **Submit Build ke Google Artifact Registry:**
   ```bash
   gcloud builds submit --tag gcr.io/PROJECT_ID_KAMU/game-api
   ```
2. **Deploy ke Cloud Run:**
   ```bash
   gcloud run deploy game-api \
     --image gcr.io/PROJECT_ID_KAMU/game-api \
     --platform managed \
     --region us-central1 \
     --allow-unauthenticated \
     --set-env-vars SUPABASE_URL=your-supabase-url,SUPABASE_SERVICE_ROLE_KEY=your-key,JWT_SECRET=your-secret
   ```
3. Salin URL HTTPS yang diberikan oleh Google Cloud Run (contoh: `https://game-api-xyz.a.run.app`) dan masukkan ke variabel `apiBaseUrl` di script `EC2Client.cs` Unity.

---

## 9. 🔬 Local Cloud Emulation & Research Lab (Floci GCP)

Sebagai bagian dari riset arsitektur cloud serverless tanpa biaya kartu kredit, proyek ini dilengkapi konfigurasi pengujian menggunakan emulator **Floci GCP**:

1. **Menjalankan Emulator GCP:**
   ```bash
   floci gcp start
   ```
2. **Mendaftarkan Service ke Emulator Cloud Run:**
   ```bash
   curl -X POST "http://localhost:4588/v2/projects/floci-local/locations/us-central1/services?serviceId=game-api" \
     -H "Content-Type: application/json" \
     -d '{"template":{"containers":[{"image":"backend-api:latest","ports":[{"containerPort":8080}]}]}}'
   ```
3. **Observabilitas via Floci UI:**
   Buka dashboard web `http://localhost:4500/console/gcp` untuk memantau status service `SUCCEEDED` dan manajemen resource Cloud Run secara visual.

---

## 10. Struktur Folder Utama

```text
My project/
├── Assets/
│   ├── Scripts/
│   │   ├── CameraFollow.cs            # Pengaturan kamera isometrik
│   │   ├── DisasterUnit.cs            # Unit pembersih puing
│   │   ├── EconomyManager.cs          # Kas uang & reward/penalti
│   │   ├── FirefighterCrew.cs         # Personel pemadam jalan kaki
│   │   ├── FireTruck.cs               # Truk pemadam & tangki air
│   │   ├── Flammable.cs               # Objek terbakar (rumah vs pohon)
│   │   ├── GameEnum.cs                # Enum status rumah & jenis unit
│   │   ├── GridManager.cs             # Input mouse & pembelian unit
│   │   ├── Hydrant.cs                 # Titik hidran isi ulang air
│   │   ├── LevelDirector.cs           # Sutradara frekuensi bencana
│   │   ├── UnitIdentity.cs            # Stats unit, mesin aus, & training
│   │   ├── UnitManager.cs             # Seleksi unit (1-3, 0 recall, Esc)
│   │   ├── UnitStatsUIManager.cs      # HUD status mesin & tangki air
│   │   ├── Database/
│   │   │   ├── AuthManager.cs         # Status autentikasi & sesi
│   │   │   ├── EC2Client.cs           # HTTP Client ke backend REST API
│   │   │   ├── GameDataManager.cs     # Provider manager (Cloud vs Local)
│   │   │   ├── ModeSelectionUI.cs     # Form UI Login & Register
│   │   │   └── backend/               # Source code REST API Node.js
│   │   │       ├── Dockerfile         # Multi-stage container recipe
│   │   │       ├── docker-compose.yml # Port mapping 3000 & 8080
│   │   │       ├── .env               # Kredensial Supabase & JWT
│   │   │       └── src/               # Express app, routes, auth, & db
│   │   ├── HQ/                        # Markas komando & UI upgrade
│   │   └── Tutorial/                  # Alur onboarding pemain pemula
└── README.md                          # Dokumentasi utama proyek
```

---

*Dokumentasi ini diperbarui secara berkala sesuai perkembangan arsitektur game dan infrastruktur cloud.*

