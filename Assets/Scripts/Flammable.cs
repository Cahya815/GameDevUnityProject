using UnityEngine;

public class Flammable : MonoBehaviour
{
    [Header("Type Settings")]
    public bool isTree = false;

    [Header("Timer Settings")]
    public float burnOutTimer = 30f; 
    private float currentBurnTimer;

    [Header("Tree Regeneration Settings")]
    public float regenerationDuration = 20f; 
    private float currentRegenTimer;

    [Header("Fire Spread Settings")]
    public bool canSpreadFire = true;
    public float spreadRadius = 60f;
    public float spreadInterval = 30f; 
    private float spreadTimer;

    [Header("Status")]
    public HouseStatus currentStatus = HouseStatus.Aman;
    public bool isBurning;
    public float fireHealth = 100f;

    [Header("Visuals")]
    public GameObject fireEffect;
    public GameObject meshNormal;
    public GameObject meshPuing; // Untuk Pohon, ini merepresentasikan "Pohon Gosong"

    void Start() {
        // Konfigurasi dinamis antara Rumah vs Pohon demi balancing game
        if (isTree) {
            burnOutTimer = 15f;           // Pohon lebih rentan dan cepat gosong
            spreadRadius = 16f;            // Merembet lebih jauh di area hutan
            spreadInterval =8f;          // Merembet lebih cepat
            regenerationDuration = 20f;   // Waktu tumbuh kembali secara otomatis
        }

        currentBurnTimer = burnOutTimer; 
        UpdateVisuals();
    }

    void Update() {
        if (TutorialManager.isTutorialActive) {
            isBurning = false;
            return;
        }

        isBurning = (currentStatus == HouseStatus.Terbakar);

        // 1. Logika Pembakaran
        if (currentStatus == HouseStatus.Terbakar) {
            currentBurnTimer -= Time.deltaTime; 

            if (currentBurnTimer <= 0) {
                currentBurnTimer = 0; 
                SetToPuing();
            }

            // 2. Logika Merembet (Spread Fire)
            if (canSpreadFire) {
                spreadTimer += Time.deltaTime;
                if (spreadTimer >= spreadInterval) {
                    spreadTimer = 0f;
                    SpreadFireToNeighbors();
                }
            }
        } else {
            spreadTimer = 0f;
        }

        // 3. Logika Regenerasi Otomatis (Hanya untuk Pohon)
        if (isTree && currentStatus == HouseStatus.Puing) {
            currentRegenTimer -= Time.deltaTime;
            if (currentRegenTimer <= 0) {
                SetToAman();
                Debug.Log($"<color=green>{gameObject.name} telah tumbuh subur kembali (Regenerasi Otomatis)!</color>");
            }
        }
    }

    // Fungsi merembetkan api ke objek Flammable lain yang aman di sekitarnya
    void SpreadFireToNeighbors() {
        if (TutorialManager.isTutorialActive) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, spreadRadius);
        foreach (var col in colliders) {
            Flammable neighbor = col.GetComponentInParent<Flammable>();
            if (neighbor != null && neighbor != this && neighbor.currentStatus == HouseStatus.Aman) {
                neighbor.SetToTerbakar();
                Debug.Log($"<color=red>Api merembet dari {gameObject.name} ke {neighbor.gameObject.name}!</color>");
            }
        }
    }

    public void SetToTerbakar() {
        if (TutorialManager.isTutorialActive) return; 

        currentStatus = HouseStatus.Terbakar;
        fireHealth = isTree ? 30f : 100f; // Pohon lebih mudah padam (HP api kecil) dibanding Rumah
        currentBurnTimer = burnOutTimer; 
        UpdateVisuals();
    }

    public void Extinguish(float p) {
        if (TutorialManager.isTutorialActive) return; 

        if (currentStatus == HouseStatus.Terbakar) {
            fireHealth -= p * Time.deltaTime;
            if (fireHealth <= 0) {
                SetToAman();
                
                // Bonus hadiah saat berhasil memadamkan api
                if (EconomyManager.instance != null) {
                    float moneyBonus = isTree ? 15f : 50f; // Memadamkan rumah memberi reward lebih besar
                    EconomyManager.instance.AddMoney(moneyBonus);
                    EconomyManager.instance.OnMissionComplete(1, 0); 
                }
            }
        }
    }

    public void CleanRubble(float s) {
        if (TutorialManager.isTutorialActive) return; 

        if (isTree) return; // Pohon tidak perlu dibersihkan secara manual!

        if (currentStatus == HouseStatus.Puing) {
            SetToAman();
            Debug.Log("<color=orange>Puing rumah berhasil dibersihkan!</color>");
            
            if (EconomyManager.instance != null) {
                EconomyManager.instance.AddMoney(50f); 
            }
        }
    }

    public void SetToAman() {
        if (TutorialManager.isTutorialActive) return; 

        currentStatus = HouseStatus.Aman;
        fireHealth = 0;
        UpdateVisuals();
    }
    
    public void UpdateVisuals() {
        if (fireEffect != null) fireEffect.SetActive(false);
        if (meshNormal != null) meshNormal.SetActive(false);
        if (meshPuing != null) meshPuing.SetActive(false);

        if (TutorialManager.isTutorialActive) {
            if (meshNormal != null) meshNormal.SetActive(true);
            return; 
        }

        switch (currentStatus) {
            case HouseStatus.Aman:
                if (meshNormal != null) meshNormal.SetActive(true);
                break;

            case HouseStatus.Terbakar:
                if (meshNormal != null) meshNormal.SetActive(true);
                if (fireEffect != null) fireEffect.SetActive(true);
                break;

            case HouseStatus.Puing:
                if (meshPuing != null) meshPuing.SetActive(true); // Pohon Gosong atau Puing Rumah
                break;
        }
    }

    public void SetToPuing() {
        if (TutorialManager.isTutorialActive) return; 

        currentStatus = HouseStatus.Puing;
        fireHealth = 0;
        UpdateVisuals();

        if (isTree) {
            currentRegenTimer = regenerationDuration;
            Debug.Log($"<color=brown>{gameObject.name} Hangus Menjadi Pohon Gosong!</color>");
        } else {
            Debug.Log($"<color=red>Rumah {gameObject.name} Hancur Menjadi Puing! Kota merugi.</color>");
            
            // Kerugian ekonomi kota karena rumah hancur
            if (EconomyManager.instance != null) {
                EconomyManager.instance.SpendMoney(150f); // Kota rugi $150
            }
        }
    }
}