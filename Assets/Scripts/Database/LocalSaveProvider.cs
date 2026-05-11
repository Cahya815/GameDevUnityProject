using UnityEngine;
using System.Threading.Tasks;

public class LocalSaveProvider : IGameDataHandler
{
    public async Task SaveMissionResult(string playerName, int firesExtinguished, float duration)
    {
        // Simulasi loading sebentar
        await Task.Delay(100); 
        
        int currentHighscore = PlayerPrefs.GetInt(playerName + "_highscore", 0);
        if (firesExtinguished > currentHighscore)
        {
            PlayerPrefs.SetInt(playerName + "_highscore", firesExtinguished);
        }
        
        Debug.Log($"[Local] Data tersimpan untuk {playerName}. Api: {firesExtinguished}");
    }

    public async Task<int> GetHighScore(string playerName)
    {
        return PlayerPrefs.GetInt(playerName + "_highscore", 0);
    }
}