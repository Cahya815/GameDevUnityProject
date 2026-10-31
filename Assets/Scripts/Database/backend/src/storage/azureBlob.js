const {
  BlobServiceClient,
  generateBlobSASQueryParameters,
  BlobSASPermissions,
  StorageSharedKeyCredential
} = require('@azure/storage-blob');
const path = require('path');
const config = require('../config');

// Standard Azurite well-known credentials (fallback only)
const AZURITE_ACCOUNT_NAME = 'devstoreaccount1';
const AZURITE_ACCOUNT_KEY =
  'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==';

/**
 * Parse Azure Storage connection string or fall back to Azurite dev storage
 */
function parseAzureConnection(connStr) {
  if (!connStr || connStr.includes('UseDevelopmentStorage=true')) {
    return {
      isDev: true,
      accountName: AZURITE_ACCOUNT_NAME,
      accountKey: AZURITE_ACCOUNT_KEY,
      baseUrl: 'http://127.0.0.1:10000/' + AZURITE_ACCOUNT_NAME
    };
  }

  const map = {};
  connStr.split(';').forEach((p) => {
    const idx = p.indexOf('=');
    if (idx !== -1) {
      map[p.substring(0, idx).trim()] = p.substring(idx + 1).trim();
    }
  });

  const accountName = map['AccountName'] || AZURITE_ACCOUNT_NAME;
  const accountKey = map['AccountKey'] || AZURITE_ACCOUNT_KEY;
  const protocol = map['DefaultEndpointsProtocol'] || 'https';
  const suffix = map['EndpointSuffix'] || 'core.windows.net';
  const baseUrl = `${protocol}://${accountName}.blob.${suffix}`;

  return { isDev: false, accountName, accountKey, baseUrl };
}

let blobServiceClient = null;

function getBlobServiceClient() {
  if (!blobServiceClient) {
    const connStr = config.AZURE_STORAGE_CONNECTION_STRING;
    try {
      if (connStr && !connStr.includes('UseDevelopmentStorage=true')) {
        blobServiceClient = BlobServiceClient.fromConnectionString(connStr);
      } else {
        const credential = new StorageSharedKeyCredential(
          AZURITE_ACCOUNT_NAME,
          AZURITE_ACCOUNT_KEY
        );
        blobServiceClient = new BlobServiceClient(
          'http://127.0.0.1:10000/' + AZURITE_ACCOUNT_NAME,
          credential
        );
      }
    } catch (err) {
      console.warn('[Azure Storage] Init warning:', err.message);
    }
  }
  return blobServiceClient;
}

/**
 * Ensure the avatar container exists and apply CORS automatically
 */
async function ensureContainerExists() {
  const containerName = config.AZURE_CONTAINER_NAME || 'avatars';
  try {
    const client = getBlobServiceClient();
    if (!client) return false;

    // 1. Automatically configure CORS so browser can PUT without manual portal setup
    try {
      await client.setProperties({
        cors: [
          {
            allowedOrigins: '*',
            allowedMethods: 'GET,POST,PUT,HEAD,OPTIONS',
            allowedHeaders: '*',
            exposedHeaders: '*',
            maxAgeInSeconds: 3600
          }
        ]
      });
      console.log('[Azure Storage] CORS rules automatically applied to storage account.');
    } catch (corsErr) {
      console.warn('[Azure Storage] CORS check warning:', corsErr.message);
    }

    // 2. Ensure container exists
    const containerClient = client.getContainerClient(containerName);
    try {
      // Try to create with public blob access
      const res = await containerClient.createIfNotExists({ access: 'blob' });
      console.log(`[Azure Storage] Container '${containerName}' ready (Public Blob, ${res.succeeded ? 'Created' : 'Already Exists'}).`);
    } catch (pubErr) {
      // Fallback if public access disabled at storage account level
      const res = await containerClient.createIfNotExists();
      console.log(`[Azure Storage] Container '${containerName}' ready (Private Container, ${res.succeeded ? 'Created' : 'Already Exists'}).`);
    }

    return true;
  } catch (err) {
    console.warn(`[Azure Storage] Notice: Container check failed (${err.message}).`);
    return false;
  }
}

/**
 * Generate a Shared Access Signature (SAS) token for direct-to-storage avatar upload
 * @param {string} userId - User ID
 * @param {string} fileName - Original uploaded file name
 * @param {string} contentType - File MIME type (image/jpeg, image/png, etc.)
 * @returns {object} { uploadUrl, publicUrl, blobName, expiresAt }
 */
function generateAvatarUploadSas(userId, fileName = 'avatar.png', contentType = 'image/png') {
  const connInfo = parseAzureConnection(config.AZURE_STORAGE_CONNECTION_STRING);
  const containerName = config.AZURE_CONTAINER_NAME || 'avatars';

  // Sanitize file extension (default .png)
  const ext = (path.extname(fileName) || '.png').toLowerCase().replace(/[^a-z0-9.]/g, '');
  const timestamp = Date.now();
  const prefix = String(userId).startsWith('usr_') ? userId : `usr_${userId}`;
  const blobName = `${prefix}_${timestamp}${ext}`;

  const sharedKey = new StorageSharedKeyCredential(connInfo.accountName, connInfo.accountKey);

  // Set SAS validity for Upload: valid from 5 minutes ago to 15 minutes from now
  const startsOn = new Date(Date.now() - 5 * 60 * 1000);
  const uploadExpiresOn = new Date(Date.now() + 15 * 60 * 1000);

  const uploadPermissions = BlobSASPermissions.parse('cw'); // Create + Write permission

  const uploadSasToken = generateBlobSASQueryParameters(
    {
      containerName,
      blobName,
      permissions: uploadPermissions,
      startsOn,
      expiresOn: uploadExpiresOn,
      contentType
    },
    sharedKey
  ).toString();

  // Generate Long-Lived Read SAS Token (1 Year) so <img> tag ALWAYS renders even if account is private!
  const readPermissions = BlobSASPermissions.parse('r');
  const readExpiresOn = new Date(Date.now() + 365 * 24 * 60 * 60 * 1000);

  const readSasToken = generateBlobSASQueryParameters(
    {
      containerName,
      blobName,
      permissions: readPermissions,
      startsOn,
      expiresOn: readExpiresOn
    },
    sharedKey
  ).toString();

  const uploadUrl = `${connInfo.baseUrl}/${containerName}/${blobName}?${uploadSasToken}`;
  const publicUrl = `${connInfo.baseUrl}/${containerName}/${blobName}?${readSasToken}`;
  const cleanUrl = `${connInfo.baseUrl}/${containerName}/${blobName}`;

  return {
    uploadUrl,
    publicUrl,
    cleanUrl,
    blobName,
    expiresAt: uploadExpiresOn.toISOString()
  };
}

module.exports = {
  parseAzureConnection,
  getBlobServiceClient,
  ensureContainerExists,
  generateAvatarUploadSas
};
