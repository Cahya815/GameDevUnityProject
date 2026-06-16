using UnityEngine;
using UnityEngine.AI;
using System.Linq; 

public class LevelDirector : MonoBehaviour
{
    public int currentLevel = 1;
    
    // Bikin private biar gak perlu drag-drop di Inspector
    private Flammable[] semuaRumah; 
    
    [Header("Grace Period after Login/Tutorial")]
    public float gracePeriodDuration = 1f;
    private float gracePeriodTimer = 0f;
    private bool hasFinishedLoginGrace = false;

    float timer;
    float nextFireIn;

    void Start() 
    { 
        // Otomatis deteksi semua objek Flammable di scene saat game dimulai
        semuaRumah = Object.FindObjectsByType<Flammable>(FindObjectsSortMode.None);
        Debug.Log("Sistem Otomatis Berhasil Menemukan " + semuaRumah.Length + " Rumah Warga!");
        
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
        // PENCEGAHAN EROR: Cek dulu array-nya kosongan atau tidak
        if (semuaRumah == null || semuaRumah.Length == 0) return;
        
        var safeHouses = System.Array.FindAll(semuaRumah, (Flammable h) => h != null && h.currentStatus == HouseStatus.Aman);
        
        if (safeHouses.Length > 0)
        {
            // Panggil fungsi SetToTerbakar() agar visualnya juga berubah
            safeHouses[Random.Range(0, safeHouses.Length)].SetToTerbakar();
        }
    }
}