using UnityEngine;
using System.Threading.Tasks;

public interface IGameDataHandler
{
    // Fungsi untuk menyimpan skor misi
    Task SaveMissionResult(string playerName, int firesExtinguished, float duration);
    
    // Fungsi untuk mengambil skor tertinggi (opsional nanti)
    Task<int> GetHighScore(string playerName);
}