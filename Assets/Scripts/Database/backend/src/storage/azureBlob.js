const {
  BlobServiceClient,
  generateBlobSASQueryParameters,
  BlobSASPermissions,
  StorageSharedKeyCredential
} = require('@azure/storage-blob');
const path = require('path');
const config = require('../config');

// Standard Azurite well-known credentials
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
        // Azurite local emulator
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
 * Ensure the avatar container exists with public blob access level
 */
async function ensureContainerExists() {
  const containerName = config.AZURE_CONTAINER_NAME || 'avatars';
  try {
    const client = getBlobServiceClient();
    if (!client) return false;
    const containerClient = client.getContainerClient(containerName);
    const createResponse = await containerClient.createIfNotExists({
      access: 'blob' // Public read access for blobs (permanent avatar URLs)
    });
    if (createResponse.succeeded) {
      console.log(`[Azure Storage] Container '${containerName}' created with public blob access.`);
    } else {
      console.log(`[Azure Storage] Container '${containerName}' is ready.`);
    }
    return true;
  } catch (err) {
    console.warn(`[Azure Storage] Notice: Container check failed (${err.message}). Is Azurite or Azure online?`);
    return false;
  }
}

/**
 * Generate a Shared Access Signature (SAS) token for direct-to-storage avatar upload
 * @param {string} userId - User ID
 * @param {string} fileName - Original uploaded file name
 * @param {string} contentType - File MIME type (image/jpeg, image/png, etc.)
 * @returns {object} { uploadUrl, publicUrl, blobName }
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

  // Set SAS validity: valid from 5 minutes ago to 15 minutes from now
  const startsOn = new Date(Date.now() - 5 * 60 * 1000);
  const expiresOn = new Date(Date.now() + 15 * 60 * 1000);

  const permissions = BlobSASPermissions.parse('cw'); // Create + Write permission

  const sasToken = generateBlobSASQueryParameters(
    {
      containerName,
      blobName,
      permissions,
      startsOn,
      expiresOn,
      contentType
    },
    sharedKey
  ).toString();

  const uploadUrl = `${connInfo.baseUrl}/${containerName}/${blobName}?${sasToken}`;
  const publicUrl = `${connInfo.baseUrl}/${containerName}/${blobName}`;

  return {
    uploadUrl,
    publicUrl,
    blobName,
    expiresAt: expiresOn.toISOString()
  };
}

module.exports = {
  parseAzureConnection,
  getBlobServiceClient,
  ensureContainerExists,
  generateAvatarUploadSas
};
