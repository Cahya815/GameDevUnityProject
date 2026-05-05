using UnityEngine;
using UnityEngine.AI;
using System.Linq; // Tambahkan ini agar filter array lebih mudah dan stabil

public class LevelDirector : MonoBehaviour
{
    public int currentLevel = 1;
    public Flammable[] allHouses; // Variabel asal (ada 's')
    
    float timer;
    float nextFireIn;

    void Start() 
    { 
        // Otomatis cari semua rumah jika lupa narik di inspector
        if (allHouses == null || allHouses.Length == 0)
        {
            allHouses = Object.FindObjectsByType<Flammable>(FindObjectsSortMode.None);
        }
        SetDifficulty(); 
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextFireIn)
        {
            TriggerFire();
            SetDifficulty(); 
            timer = 0;
        }
    }

    void SetDifficulty()
    {
        if (currentLevel == 1) nextFireIn = Random.Range(15f, 25f);
        else if (currentLevel == 2) nextFireIn = Random.Range(10f, 15f);
        else nextFireIn = Random.Range(5f, 10f);
    }

    void TriggerFire()
    {
        // Perbaikan typo: allHouses (pakai s)
        // Perbaikan logika: Gunakan bantuan (Flammable h) agar C# tidak bingung tipe datanya
        var safeHouses = System.Array.FindAll(allHouses, (Flammable h) => h.currentStatus == HouseStatus.Aman);
        
        if (safeHouses.Length > 0)
        {
            // Panggil fungsi SetToTerbakar() agar visualnya juga berubah
            safeHouses[Random.Range(0, safeHouses.Length)].SetToTerbakar();
        }
    }
}