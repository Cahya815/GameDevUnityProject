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
            Debug.LogError("AuthManager tidak ditemukan!");
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
            Debug.LogError("Data handler belum diinisialisasi!");
            return;
        }

        string playerName = AuthManager.instance.playerName;
        await _dataHandler.SaveMissionResult(playerName, firesExtinguished, duration);
    }
}