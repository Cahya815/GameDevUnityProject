using UnityEngine;
using System.Threading.Tasks;

public static class GameSession
{
    public static string playerName;
    public static bool isOnlineMode;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;
    private IGameDataHandler _dataHandler;

    private void Awake()
{
    if (instance == null)
    {
        instance = this;
        // Hapus DontDestroyOnLoad
    }
    else
    {
        Destroy(gameObject);
    }
}

    private void Start()
    {
        // Jangan inisialisasi di Start, tunggu sampai mode dipilih
        Debug.Log("GameDataManager siap menunggu mode selection");
    }

    public void InitializeDataHandler()
    {
        if (AuthManager.instance == null)
        {
            Debug.LogWarning("AuthManager tidak ditemukan! Default ke Local Save.");
            _dataHandler = new LocalSaveProvider();
            return;
        }

        if (AuthManager.instance.isOnlineMode)
        {
            _dataHandler = new SupabaseProvider();
            Debug.Log("<color=cyan>Menggunakan Online Database</color>");
        }
        else
        {
            _dataHandler = new LocalSaveProvider();
            Debug.Log("<color=yellow>Menggunakan Local Storage</color>");
        }
    }

    public async void SaveGameResult(int firesExtinguished, float duration)
    {
        if (_dataHandler == null)
        {
            Debug.LogWarning("Data handler belum diinisialisasi! Menginisialisasi default...");
            InitializeDataHandler();
        }

        string playerName = AuthManager.instance != null ? AuthManager.instance.playerName : "Pemain1";

        // Save to primary data handler (Supabase or Local)
        await _dataHandler.SaveMissionResult(playerName, firesExtinguished, duration);

        // Also save to EC2 backend API for leaderboard
        await SaveToEC2Backend(playerName, firesExtinguished, (int)duration);
    }

    private async Task SaveToEC2Backend(string playerName, int firesExtinguished, int duration)
    {
        try
        {
            EC2Client.Instance.SetPlayerName(playerName);
            var result = await EC2Client.Instance.SaveGameLog(firesExtinguished, duration);
            Debug.Log($"<color=green>✓ Saved to EC2 Backend! ID: {result.id}</color>");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"<color=yellow>EC2 Backend save failed (offline?): {ex.Message}</color>");
            // Don't throw - allow game to continue even if backend is down
        }
    }

    public async Task<LeaderboardEntry[]> GetLeaderboard(int limit = 10)
    {
        try
        {
            return await EC2Client.Instance.GetTopLeaderboard(limit);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to fetch leaderboard: {ex.Message}");
            return new LeaderboardEntry[0];
        }
    }

    public async Task<GameLogResponse[]> GetPlayerHistory(string playerName = null)
    {
        try
        {
            return await EC2Client.Instance.GetPlayerLogs(playerName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to fetch player history: {ex.Message}");
            return new GameLogResponse[0];
        }
    }
}