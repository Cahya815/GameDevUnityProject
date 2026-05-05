using UnityEngine;

public class FireManager : MonoBehaviour {
    public Flammable[] allHouses;
    public float fireInterval = 10f;

    void Start() {
    // 1. Cari otomatis semua objek di map yang punya script Flammable
    // Kamu tidak perlu lagi narik-narik di Inspector!
    allHouses = Object.FindObjectsByType<Flammable>(FindObjectsSortMode.None);

    // 2. Mulai siklus api
    InvokeRepeating("TriggerRandomFire", 5f, fireInterval);
    
    Debug.Log("Sistem menemukan " + allHouses.Length + " rumah di map.");
}

    void TriggerRandomFire() {
        // Cari rumah yang belum terbakar
        var availableHouses = System.Array.FindAll(allHouses, h => !h.isBurning);
        if(availableHouses.Length > 0) {
            int index = Random.Range(0, availableHouses.Length);
            availableHouses[index].isBurning = true;
            availableHouses[index].fireHealth = 100f; // Reset darah api
        }
    }
}