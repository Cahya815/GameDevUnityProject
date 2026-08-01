using UnityEngine;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;
    public string playerName { get; private set; }
    public bool isOnlineMode { get; private set; }
    public bool isLoggedIn { get; private set; }

   private void Awake()
{
    if (instance == null)
    {
        instance = this;
        // Hapus DontDestroyOnLoad - biarkan destroy saat scene load
    }
    else
    {
        Destroy(gameObject);
    }
}

    public Task LoginOnline(string username, string password)
    {
        playerName = username;
        isOnlineMode = false; // belum terverifikasi — menunggu balikan Google via deep link
        isLoggedIn = false;
        Debug.Log($"<color=cyan>Membuka browser untuk login Google sebagai: {username}</color>");

        if (DeepLinkManager.instance == null)
            Debug.LogError("DeepLinkManager tidak ditemukan di scene! Tambahkan ke GameObject scene menu.");
        else
            DeepLinkManager.instance.StartGoogleOAuth();

        return Task.CompletedTask;
    }

    // Dipanggil DeepLinkManager setelah token OAuth diterima
    public void CompleteOnlineLogin()
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "PemainGoogle"; // cold start via deep link, nama belum dipilih
        isOnlineMode = true;
        isLoggedIn = true;
        Debug.Log($"<color=cyan>Login Online sukses sebagai: {playerName}</color>");

        if (ModeSelectionUI.instance != null)
            ModeSelectionUI.instance.FinishOnlineLogin();
    }

    public void SelectOfflineMode(string username)
    {
        playerName = username;
        isOnlineMode = false;
        isLoggedIn = true;
        Debug.Log($"<color=yellow>Mode Offline untuk: {username}</color>");
    }
}