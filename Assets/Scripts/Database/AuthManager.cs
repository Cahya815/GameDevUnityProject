using UnityEngine;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;
    public string playerName { get; private set; }
    public string userId { get; private set; }
    public bool isOnlineMode { get; private set; }
    public bool isLoggedIn { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Check if user was previously authenticated
        if (EC2Client.Instance.IsAuthenticated())
        {
            playerName = EC2Client.Instance.GetCurrentUsername();
            userId = EC2Client.Instance.GetCurrentUserId();
            isOnlineMode = true;
            isLoggedIn = true;
            Debug.Log($"[AuthManager] Resuming session for: {playerName}");
        }
    }

    /// <summary>
    /// Register new user dengan email + username + password
    /// </summary>
    public async Task<bool> RegisterAsync(string email, string username, string password)
    {
        try
        {
            Debug.Log("[AuthManager] Registering user...");
            var authResponse = await EC2Client.Instance.RegisterAsync(email, username, password);

            playerName = authResponse.username;
            userId = authResponse.userId;
            isOnlineMode = true;
            isLoggedIn = true;

            Debug.Log($"[AuthManager] Registration successful. User: {playerName}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthManager] Registration failed: {ex.Message}");
            isLoggedIn = false;
            return false;
        }
    }

    /// <summary>
    /// Login user dengan email + password
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            Debug.Log("[AuthManager] Logging in user...");
            var authResponse = await EC2Client.Instance.LoginAsync(email, password);

            playerName = authResponse.username;
            userId = authResponse.userId;
            isOnlineMode = true;
            isLoggedIn = true;

            Debug.Log($"[AuthManager] Login successful. User: {playerName}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthManager] Login failed: {ex.Message}");
            isLoggedIn = false;
            return false;
        }
    }

    /// <summary>
    /// Login dengan Google OAuth (legacy method)
    /// </summary>
    public Task LoginOnline(string username, string password)
    {
        playerName = username;
        isOnlineMode = false;
        isLoggedIn = false;
        Debug.Log($"[AuthManager] Opening browser for Google login as: {username}");

        if (DeepLinkManager.instance == null)
            Debug.LogError("DeepLinkManager not found in scene!");
        else
            DeepLinkManager.instance.StartGoogleOAuth();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by DeepLinkManager setelah OAuth token diterima
    /// </summary>
    public void CompleteOnlineLogin()
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "GoogleUser";
        isOnlineMode = true;
        isLoggedIn = true;
        Debug.Log($"[AuthManager] Google login complete. User: {playerName}");
    }

    /// <summary>
    /// Logout dan clear JWT token
    /// </summary>
    public void Logout()
    {
        EC2Client.Instance.Logout();
        playerName = "";
        userId = "";
        isOnlineMode = false;
        isLoggedIn = false;
        Debug.Log("[AuthManager] User logged out");
    }

    /// <summary>
    /// Check if user is authenticated (baik JWT maupun offline)
    /// </summary>
    public bool IsAuthenticated()
    {
        return isLoggedIn && !string.IsNullOrEmpty(playerName);
    }

    /// <summary>
    /// Get user profile (dari server)
    /// </summary>
    public async Task<UserProfile> GetUserProfileAsync()
    {
        try
        {
            if (!EC2Client.Instance.IsAuthenticated())
            {
                throw new System.Exception("Not authenticated. Please login first.");
            }
            return await EC2Client.Instance.GetUserProfileAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthManager] GetUserProfile failed: {ex.Message}");
            throw;
        }
    }
}