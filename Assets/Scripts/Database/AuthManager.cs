using UnityEngine;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;
    public string playerName { get; private set; }
    public bool isOnlineMode { get; private set; }

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
        // TODO: Implementasi login ke Supabase
        playerName = username;
        isOnlineMode = true;
        Debug.Log($"<color=cyan>Login Online sebagai: {username}</color>");
        return Task.CompletedTask;
    }

    public void SelectOfflineMode(string username)
    {
        playerName = username;
        isOnlineMode = false;
        Debug.Log($"<color=yellow>Mode Offline untuk: {username}</color>");
    }
}