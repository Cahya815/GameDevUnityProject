using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

public class SupabaseProvider : IGameDataHandler
{
    // Pastikan pakai tanda kutip "" dan diakhiri ;
    private string url = "https://asxsyvbnuxmhacxbqazb.supabase.co/rest/v1/game_logs";
    private string apiKey = "sb_publishable_np4v_4z8KSZGLtvbcPHeJQ_jTdoCCHi";

    public async Task SaveMissionResult(string playerName, int firesExtinguished, float duration)
    {
        // 1. Format JSON (Sesuaikan dengan nama kolom di Supabase)
        string json = "{\"player_name\":\"" + playerName + "\", \"fires_extinguished\":" + firesExtinguished + ", \"duration\":" + duration + "}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Gagal Kirim ke Supabase: " + request.error + " | Respon: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("<color=green>Berhasil simpan ke Supabase!</color>");
            }
        }
    }

    public async Task<int> GetHighScore(string playerName)
    {
        return 0; // Sementara
    }
}