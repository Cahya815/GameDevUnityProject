using UnityEngine;
using System.Threading.Tasks;

private IGameDataHandler _dataHandler;

private void Start()
{
    // Pilih provider berdasarkan mode
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
    string playerName = AuthManager.instance.playerName;
    await _dataHandler.SaveMissionResult(playerName, firesExtinguished, duration);
}


public interface IGameDataHandler
{
    // Fungsi untuk menyimpan skor misi
    Task SaveMissionResult(string playerName, int firesExtinguished, float duration);
    
    // Fungsi untuk mengambil skor tertinggi (opsional nanti)
    Task<int> GetHighScore(string playerName);
}