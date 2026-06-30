using UnityEngine;

public class HQController : MonoBehaviour
{
    public string buildingName = "Base HQ";
    public int hqLevel = 1;
    public static int currentHQLevel = 1; // Untuk akses global
    public int maxHQLevel = 4; // Contoh, bisa disesuaikan
    public float health = 500f;

    // Fungsi yang bakal dipanggil pas HQ diklik
    void Awake()
    {
        currentHQLevel = hqLevel;
    }

    public void OpenMenu()
    {
        // Panggil Manager UI (kita buat di bawah)
        HQUIManager.instance.OpenMenu(this);
    }

    public void Interact() {
    HQUIManager.instance.OpenMenu(this);

    // Jika sedang tutorial step 1, lanjut ke step berikutnya
    if (FindObjectOfType<TutorialManager>() != null) {
        FindObjectOfType<TutorialManager>().NextStep();
    }
}   

    public void UpgradeBuilding()
    {
        if (currentHQLevel < maxHQLevel)
        {
            float upgradeCost = GetUpgradeCost(currentHQLevel + 1); // Biaya untuk level berikutnya

            if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(upgradeCost))
            {
                currentHQLevel++;
                hqLevel = currentHQLevel; // Update hqLevel agar sesuai dengan currentHQLevel
                health += 200f;
                Debug.Log($"HQ Upgraded to Level: {currentHQLevel}. Biaya: ${upgradeCost}.");
            }
            else
            {
                Debug.LogWarning($"Uang tidak cukup untuk upgrade HQ ke Level {currentHQLevel + 1}! Dibutuhkan: ${upgradeCost}.");
            }
        }
        else
        {
            Debug.Log("HQ is already at max level (" + maxHQLevel + ")");
        }
    }

    public float GetUpgradeCost(int targetLevel)
    {
        switch (targetLevel)
        {
            case 2: return 100f;
            case 3: return 250f;
            case 4: return 999f;
            default: return 0f; // Tidak ada biaya jika level di luar range atau sudah max
        }
    }
}