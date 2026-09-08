using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class EC2Client : MonoBehaviour
{
    private static EC2Client instance;
    private HttpClient httpClient;

    [SerializeField] private string apiBaseUrl = "http://localhost:3000";
    [SerializeField] private float requestTimeout = 30f;

    private string cachedPlayerName = "";
    private string jwtToken = ""; // JWT token untuk authenticated requests
    private bool isInitialized = false;

    private const string JWT_TOKEN_KEY = "game_pemadam_jwt_token";
    private const string USER_ID_KEY = "game_pemadam_user_id";
    private const string USERNAME_KEY = "game_pemadam_username";

    public static EC2Client Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EC2Client>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EC2Client");
                    instance = go.AddComponent<EC2Client>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeClient();
    }

    private void InitializeClient()
    {
        if (isInitialized) return;
        httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(requestTimeout);
        isInitialized = true;

        // Load JWT token from PlayerPrefs if exists
        if (PlayerPrefs.HasKey(JWT_TOKEN_KEY))
        {
            jwtToken = PlayerPrefs.GetString(JWT_TOKEN_KEY);
            Debug.Log("[EC2Client] JWT token loaded from PlayerPrefs");
        }

        Debug.Log($"[EC2Client] Initialized. Base URL: {apiBaseUrl}");
    }

    public void SetBaseUrl(string url)
    {
        apiBaseUrl = url;
        Debug.Log($"[EC2Client] Base URL changed to: {apiBaseUrl}");
    }

    public void SetPlayerName(string playerName)
    {
        cachedPlayerName = playerName;
    }

    // ===== AUTHENTICATION METHODS =====

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(string email, string username, string password)
    {
        try
        {
            var payload = new RegisterRequest
            {
                email = email,
                username = username,
                password = password
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{apiBaseUrl}/auth/register", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<AuthResponse>>(responseBody);
                if (result.success && result.data != null)
                {
                    SaveJWTToken(result.data.token, result.data.userId, result.data.username);
                    Debug.Log($"[EC2Client] Registration successful. User: {result.data.username}");
                    return result.data;
                }
            }

            // Parse error response dari server
            string errorMessage = $"Registrasi gagal (HTTP {response.StatusCode})";
            try
            {
                var errorResult = JsonUtility.FromJson<ApiErrorResponse>(responseBody);
                if (errorResult != null && !string.IsNullOrEmpty(errorResult.message))
                    errorMessage = errorResult.message;
            }
            catch { }

            Debug.LogError($"[EC2Client] Registration failed. Status: {response.StatusCode}. Response: {responseBody}");
            throw new Exception(errorMessage);
        }
        catch (Exception ex)
        {
            // Provide better error messages based on exception type
            string errorMsg = ex.Message;

            if (ex is HttpRequestException)
            {
                errorMsg = "Server tidak dapat diakses. Periksa koneksi internet atau URL backend.";
            }
            else if (ex is TaskCanceledException)
            {
                errorMsg = "Request timeout. Server tidak merespons dalam waktu 30 detik.";
            }
            else if (errorMsg.Contains("An error occurred while sending the request"))
            {
                errorMsg = "Gagal terhubung ke server. Pastikan backend sedang berjalan di " + apiBaseUrl;
            }

            Debug.LogError($"[EC2Client] Register Error: {errorMsg}");
            throw new Exception(errorMsg);
        }
    }

    /// <summary>
    /// Login user dengan email dan password
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            var payload = new LoginRequest
            {
                email = email,
                password = password
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{apiBaseUrl}/auth/login", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<AuthResponse>>(responseBody);
                if (result.success && result.data != null)
                {
                    SaveJWTToken(result.data.token, result.data.userId, result.data.username);
                    cachedPlayerName = result.data.username;

                    Debug.Log($"[EC2Client] Login successful. User: {result.data.username}");
                    return result.data;
                }
            }

            // Parse error response dari server
            string errorMessage = $"Login gagal (HTTP {response.StatusCode})";
            try
            {
                var errorResult = JsonUtility.FromJson<ApiErrorResponse>(responseBody);
                if (errorResult != null && !string.IsNullOrEmpty(errorResult.message))
                    errorMessage = errorResult.message;
            }
            catch { }

            Debug.LogError($"[EC2Client] Login failed. Status: {response.StatusCode}. Response: {responseBody}");
            throw new Exception(errorMessage);
        }
        catch (Exception ex)
        {
            // Provide better error messages based on exception type
            string errorMsg = ex.Message;

            if (ex is HttpRequestException)
            {
                errorMsg = "Server tidak dapat diakses. Periksa koneksi internet atau URL backend.";
            }
            else if (ex is TaskCanceledException)
            {
                errorMsg = "Request timeout. Server tidak merespons dalam waktu 30 detik.";
            }
            else if (errorMsg.Contains("An error occurred while sending the request"))
            {
                errorMsg = "Gagal terhubung ke server. Pastikan backend sedang berjalan di " + apiBaseUrl;
            }

            Debug.LogError($"[EC2Client] Login Error: {errorMsg}");
            throw new Exception(errorMsg);
        }
    }

    /// <summary>
    /// Get current user profile (protected route, memerlukan JWT token)
    /// </summary>
    public async Task<UserProfile> GetUserProfileAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token not found. Please login first.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{apiBaseUrl}/auth/me");
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            var response = await httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<UserProfile>>(responseBody);
                Debug.Log($"[EC2Client] User profile retrieved: {result.data.username}");
                return result.data;
            }

            Debug.LogError($"[EC2Client] Get profile failed. Status: {response.StatusCode}");
            throw new Exception($"Get profile failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EC2Client] GetUserProfile Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Logout - clear JWT token
    /// </summary>
    public void Logout()
    {
        PlayerPrefs.DeleteKey(JWT_TOKEN_KEY);
        PlayerPrefs.DeleteKey(USER_ID_KEY);
        PlayerPrefs.DeleteKey(USERNAME_KEY);
        jwtToken = "";
        cachedPlayerName = "";
        Debug.Log("[EC2Client] Logged out. JWT token cleared.");
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(jwtToken);
    }

    // ===== GAME DATA METHODS =====

    public async Task<SaveGameResponse> SaveGameLog(int firesExtinguished, int duration)
    {
        try
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token not found. Please login first.");
            }

            var payload = new SaveGameRequest
            {
                playerName = cachedPlayerName,
                firesExtinguished = firesExtinguished,
                duration = duration
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{apiBaseUrl}/v1/saves");
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");
            request.Content = content;

            var response = await httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<SaveGameResponse>>(responseBody);
                Debug.Log($"[EC2Client] Game saved. ID: {result.data.id}");
                return result.data;
            }
            else
            {
                Debug.LogError($"[EC2Client] Save failed. Status: {response.StatusCode}. Response: {responseBody}");
                throw new Exception($"Save failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EC2Client] SaveGameLog Error: {ex.Message}");
            throw;
        }
    }

    public async Task<GameLogResponse[]> GetPlayerLogs(string playerName = null)
    {
        try
        {
            string name = playerName ?? cachedPlayerName;
            string url = $"{apiBaseUrl}/v1/saves?playerName={Uri.EscapeDataString(name)}";

            var response = await httpClient.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<GameLogResponse[]>>(responseBody);
                Debug.Log($"[EC2Client] Retrieved {result.data.Length} logs for: {name}");
                return result.data;
            }
            else
            {
                Debug.LogError($"[EC2Client] Get logs failed. Status: {response.StatusCode}");
                throw new Exception($"Get logs failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EC2Client] GetPlayerLogs Error: {ex.Message}");
            throw;
        }
    }

    public async Task<LeaderboardEntry[]> GetTopLeaderboard(int limit = 10)
    {
        try
        {
            string url = $"{apiBaseUrl}/v1/leaderboard/top?limit={limit}";
            var response = await httpClient.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonUtility.FromJson<ApiResponse<LeaderboardEntry[]>>(responseBody);
                Debug.Log($"[EC2Client] Retrieved {result.data.Length} leaderboard entries");
                return result.data;
            }
            else
            {
                Debug.LogError($"[EC2Client] Leaderboard failed. Status: {response.StatusCode}");
                throw new Exception($"Leaderboard failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EC2Client] GetTopLeaderboard Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> HealthCheck()
    {
        try
        {
            var response = await httpClient.GetAsync($"{apiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ===== HELPER METHODS =====

    private void SaveJWTToken(string token, string userId, string username)
    {
        jwtToken = token;
        PlayerPrefs.SetString(JWT_TOKEN_KEY, token);
        PlayerPrefs.SetString(USER_ID_KEY, userId);
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.Save();
        Debug.Log($"[EC2Client] JWT token saved to PlayerPrefs. User: {username}");
    }

    public string GetCurrentUserId()
    {
        return PlayerPrefs.GetString(USER_ID_KEY, "");
    }

    public string GetCurrentUsername()
    {
        return PlayerPrefs.GetString(USERNAME_KEY, "");
    }

    void OnDestroy()
    {
        if (httpClient != null) httpClient.Dispose();
    }
}
