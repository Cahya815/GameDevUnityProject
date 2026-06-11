using UnityEngine;
using UnityEngine.AI;
using System.Linq; 

public class LevelDirector : MonoBehaviour
{
    public int currentLevel = 1;
    public Flammable[] allHouses; // Variabel asal (ada 's')
    
    [Header("Grace Period after Login/Tutorial")]
    public float gracePeriodDuration = 1f;
    private float gracePeriodTimer = 0f;
    private bool hasFinishedLoginGrace = false;

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
        gracePeriodTimer = gracePeriodDuration;
    }

    void Update()
    {
        if (TutorialManager.isTutorialActive) return; // Api nggak bakal spawn
        
        // Jeda persiapan 10 detik setelah login/tutorial selesai
        if (!hasFinishedLoginGrace)
        {
            gracePeriodTimer -= Time.deltaTime;
            if (gracePeriodTimer <= 0)
            {
                hasFinishedLoginGrace = true;
                timer = 0f;
                Debug.Log("<color=cyan>Jeda persiapan 10 detik selesai! Api mulai bermunculan.</color>");
            }
            return;
        }

        timer += Time.deltaTime; // Hanya bertambah sekali per frame sekarang (diperbaiki!)
        
        if (timer >= nextFireIn)
        {
            TriggerFire();
            SetDifficulty(); 
            timer = 0;
        }
    }

    void SetDifficulty()
    {
        if (currentLevel == 1) nextFireIn = Random.Range(2f, 5f);
        else if (currentLevel == 2) nextFireIn = Random.Range(10f, 15f);
        else nextFireIn = Random.Range(2f, 5f);
    }

    void TriggerFire()
    {
        
        var safeHouses = System.Array.FindAll(allHouses, (Flammable h) => h.currentStatus == HouseStatus.Aman);
        
        if (safeHouses.Length > 0)
        {
            // Panggil fungsi SetToTerbakar() agar visualnya juga berubah
            safeHouses[Random.Range(0, safeHouses.Length)].SetToTerbakar();
        }
    }
}