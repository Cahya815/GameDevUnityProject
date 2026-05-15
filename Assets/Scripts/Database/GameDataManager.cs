using UnityEngine;
using System.Threading.Tasks;

public class GameDataManager : MonoBehaviour
{
    private IGameDataHandler _dataHandler;

    private void Start()
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
        if (AuthManager.instance == null || _dataHandler == null)
        {
            Debug.LogError("Data handler belum diinisialisasi!");
            return;
        }

        string playerName = AuthManager.instance.playerName;
        await _dataHandler.SaveMissionResult(playerName, firesExtinguished, duration);
    }
}