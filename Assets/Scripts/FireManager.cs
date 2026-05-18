using UnityEngine;

public class FireManager : MonoBehaviour {
    public Flammable[] allHouses;
    public float fireInterval = 10f;

    void Start() {
        allHouses = Object.FindObjectsByType<Flammable>(FindObjectsSortMode.None);

        // Jangan mulai fire jika tutorial aktif
        if (!TutorialManager.isTutorialActive)
        {
            InvokeRepeating("TriggerRandomFire", 5f, fireInterval);
            Debug.Log("Sistem menemukan " + allHouses.Length + " rumah di map.");
        }
        else
        {
            Debug.Log("Tutorial aktif, fire system ditunda.");
        }
    }

    void TriggerRandomFire() {
        // Pause jika tutorial masih aktif
        if (TutorialManager.isTutorialActive)
        {
            return;
        }

        var availableHouses = System.Array.FindAll(allHouses, h => !h.isBurning);
        if(availableHouses.Length > 0) {
            int index = Random.Range(0, availableHouses.Length);
            availableHouses[index].isBurning = true;
            availableHouses[index].fireHealth = 100f;
        }
    }
}