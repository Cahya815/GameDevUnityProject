const express = require('express');
const bcrypt = require('bcryptjs');
const multer = require('multer');
const { authenticateToken } = require('../middleware/auth');
const db = require('../db/supabase');
const azureBlob = require('../storage/azureBlob');

const upload = multer({
  storage: multer.memoryStorage(),
  limits: { fileSize: 2 * 1024 * 1024 } // 2MB limit
});

const router = express.Router();

/**
 * GET /v1/profile - Ambil data profil lengkap pemain
 * Header: Authorization: Bearer <jwt_token>
 */
router.get('/', authenticateToken, async (req, res) => {
  try {
    const user = await db.getUserProfile(req.user.userId);
    if (!user) {
      return res.status(404).json({
        success: false,
        data: null,
        error: 'User tidak ditemukan'
      });
    }

    return res.status(200).json({
      success: true,
      data: {
        id: user.id,
        email: user.email,
        username: user.username,
        avatar_url: user.avatar_url || null,
        createdAt: user.created_at
      }
    });
  } catch (err) {
    console.error('[GET /v1/profile Error]', err.message);
    return res.status(500).json({
      success: false,
      data: null,
      error: 'Gagal mengambil data profil: ' + err.message
    });
  }
});

/**
 * PUT /v1/profile/nickname - Perbarui Riot ID / Nickname tampilan pemain
 * Header: Authorization: Bearer <jwt_token>
 * Body: { nickname: "KomandanRendy" }
 */
router.put('/nickname', authenticateToken, async (req, res) => {
  try {
    const { nickname } = req.body;
    if (!nickname || typeof nickname !== 'string' || nickname.trim().length === 0) {
      return res.status(400).json({
        success: false,
        error: 'Nickname tidak boleh kosong'
      });
    }

    const trimmed = nickname.trim();
    if (trimmed.length < 3 || trimmed.length > 30) {
      return res.status(400).json({
        success: false,
        error: 'Nickname harus antara 3 hingga 30 karakter'
      });
    }

    // Periksa apakah username sudah dipakai pemain lain
    const taken = await db.isUsernameTaken(trimmed, req.user.userId);
    if (taken) {
      return res.status(400).json({
        success: false,
        error: 'Nickname sudah terpakai oleh pemain lain'
      });
    }

    const updated = await db.updateUserNickname(req.user.userId, trimmed);
    return res.status(200).json({
      success: true,
      data: { username: updated.username },
      message: 'Nickname berhasil diperbarui'
    });
  } catch (err) {
    console.error('[PUT /v1/profile/nickname Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Gagal memperbarui nickname: ' + err.message
    });
  }
});

/**
 * PUT /v1/profile/password - Perbarui kata sandi dengan verifikasi kata sandi lama
 * Header: Authorization: Bearer <jwt_token>
 * Body: { currentPassword: "...", newPassword: "..." }
 */
router.put('/password', authenticateToken, async (req, res) => {
  try {
    const { currentPassword, newPassword } = req.body;

    if (!currentPassword || !newPassword) {
      return res.status(400).json({
        success: false,
        error: 'Kata sandi saat ini dan kata sandi baru wajib diisi'
      });
    }

    if (newPassword.length < 8) {
      return res.status(400).json({
        success: false,
        error: 'Kata sandi baru minimal 8 karakter'
      });
    }

    // Ambil password_hash saat ini
    const user = await db.getUserWithPassword(req.user.userId);
    if (!user) {
      return res.status(404).json({
        success: false,
        error: 'User tidak ditemukan'
      });
    }

    // Verifikasi kata sandi lama via bcrypt
    const valid = await bcrypt.compare(currentPassword, user.password_hash);
    if (!valid) {
      return res.status(400).json({
        success: false,
        error: 'Kata sandi saat ini salah'
      });
    }

    // Hash kata sandi baru dan simpan
    const newHash = await bcrypt.hash(newPassword, 10);
    await db.updateUserPassword(req.user.userId, newHash);

    return res.status(200).json({
      success: true,
      message: 'Kata sandi berhasil diperbarui'
    });
  } catch (err) {
    console.error('[PUT /v1/profile/password Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Gagal memperbarui kata sandi: ' + err.message
    });
  }
});

/**
 * POST /v1/profile/avatar/sas-upload - Minta tiket SAS upload langsung ke Azure Blob Storage
 * Header: Authorization: Bearer <jwt_token>
 * Body: { fileName: "profile.png", contentType: "image/png" }
 */
router.post('/avatar/sas-upload', authenticateToken, async (req, res) => {
  try {
    const { fileName, contentType } = req.body;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    const mime = (contentType || 'image/png').toLowerCase();

    if (!allowedTypes.includes(mime)) {
      return res.status(400).json({
        success: false,
        error: 'Format file tidak didukung. Harap unggah JPG, PNG, WebP, atau GIF.'
      });
    }

    // Pastikan container exists (non-blocking jika offline)
    azureBlob.ensureContainerExists().catch(() => {});

    // Buat SAS upload URL
    const sasData = azureBlob.generateAvatarUploadSas(
      req.user.userId,
      fileName || 'avatar.png',
      mime
    );

    return res.status(200).json({
      success: true,
      uploadUrl: sasData.uploadUrl,
      publicUrl: sasData.publicUrl,
      blobName: sasData.blobName,
      expiresAt: sasData.expiresAt
    });
  } catch (err) {
    console.error('[POST /v1/profile/avatar/sas-upload Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Gagal membuat tiket SAS upload: ' + err.message
    });
  }
});

/**
 * POST /v1/profile/avatar/confirm - Konfirmasi avatar berhasil diunggah ke Azure dan simpan URL ke DB
 * Header: Authorization: Bearer <jwt_token>
 * Body: { publicUrl: "https://.../avatars/usr_1.png" }
 */
router.post('/avatar/confirm', authenticateToken, async (req, res) => {
  try {
    const { publicUrl } = req.body;
    if (!publicUrl || typeof publicUrl !== 'string') {
      return res.status(400).json({
        success: false,
        error: 'publicUrl wajib disertakan'
      });
    }

    const updated = await db.updateUserAvatar(req.user.userId, publicUrl);

    return res.status(200).json({
      success: true,
      data: {
        avatar_url: updated.avatar_url
      },
      message: 'Foto profil berhasil disimpan ke database'
    });
  } catch (err) {
    console.error('[POST /v1/profile/avatar/confirm Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Gagal menyimpan foto profil: ' + err.message
    });
  }
});

/**
 * POST /v1/profile/avatar - Upload avatar binary directly via server to Azure Blob Storage
 * Header: Authorization: Bearer <jwt_token>
 * Body: multipart/form-data (field: 'avatar')
 */
router.post('/avatar', authenticateToken, upload.single('avatar'), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({
        success: false,
        error: 'File avatar tidak ditemukan dalam request'
      });
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!allowedTypes.includes(req.file.mimetype.toLowerCase())) {
      return res.status(400).json({
        success: false,
        error: 'Format file tidak didukung. Harap unggah JPG, PNG, WebP, atau GIF.'
      });
    }

    // Direct upload buffer to Azure Blob Storage
    const { publicUrl, blobName } = await azureBlob.uploadAvatarDirect(
      req.user.userId,
      req.file.buffer,
      req.file.originalname,
      req.file.mimetype
    );

    // Update avatar_url in Supabase
    const updated = await db.updateUserAvatar(req.user.userId, publicUrl);

    return res.status(200).json({
      success: true,
      message: 'Foto avatar berhasil diunggah ke Microsoft Azure Blob Storage!',
      data: {
        avatar_url: updated.avatar_url,
        blobName
      }
    });
  } catch (err) {
    console.error('[POST /v1/profile/avatar Error]', err.message);
    return res.status(500).json({
      success: false,
      error: 'Gagal mengunggah foto profil: ' + err.message
    });
  }
});

module.exports = router;

