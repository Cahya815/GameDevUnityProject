using UnityEngine;
using UnityEngine.Networking;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager instance;
    public static string AccessToken { get; private set; }

    private const string AuthorizeUrl = "https://asxsyvbnuxmhacxbqazb.supabase.co/auth/v1/authorize";
    private const string RedirectUrl = "myfiregame://auth-callback";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Application.deepLinkActivated += OnDeepLinkActivated;

        // Cek saat game dibuka langsung dari deep link (cold start)
        if (!string.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLinkActivated(Application.absoluteURL);
    }

    // Dipanggil tombol "Online" — buka browser untuk login Google
    public void StartGoogleOAuth()
    {
        // Implicit flow: token dikembalikan di fragment URL (#access_token=...)
        string redirect = UnityWebRequest.EscapeURL(RedirectUrl);
        Application.OpenURL($"{AuthorizeUrl}?provider=google&redirect_to={redirect}");
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("Deep link diterima: " + url);

        // Balikan OAuth: myfiregame://auth-callback#access_token=xxx&expires_in=3600&token_type=bearer
        string[] parts = url.Split('#');
        if (parts.Length < 2)
            return;

        foreach (string kv in parts[1].Split('&'))
        {
            string[] pair = kv.Split('=');
            if (pair.Length != 2 || pair[0] != "access_token")
                continue;

            AccessToken = pair[1];
            PlayerPrefs.SetString("access_token", AccessToken);
            PlayerPrefs.Save();

            Debug.Log("<color=cyan>Login Google OK! Token tersimpan.</color>");
            if (AuthManager.instance != null)
                AuthManager.instance.CompleteOnlineLogin();
            break;
        }
    }
}
