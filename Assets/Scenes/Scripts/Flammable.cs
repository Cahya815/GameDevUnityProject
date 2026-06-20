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
    [Tooltip("Variasi waktu acak dalam detik (min: interval - variance, max: interval + variance) agar penyebaran lebih dinamis")]
    public float spreadVariance = 10f;
    private float currentSpreadInterval;
    private float spreadTimer;

    [Header("Status")]
    public HouseStatus currentStatus = HouseStatus.Aman;
    public bool isBurning;
    public float fireHealth = 100f;

    [Header("Visuals")]
    public GameObject fireEffect;
    public GameObject meshNormal;
    public GameObject meshPuing; // Untuk Pohon, ini merepresentasikan "Pohon Gosong"

    [Header("Animal Emergency Visuals")]
    public GameObject snakeVisual;
    public GameObject horseVisual;

    public bool IsActiveFirefighterEmergency()
    {
        return currentStatus == HouseStatus.Terbakar || currentStatus == HouseStatus.AdaUlar || currentStatus == HouseStatus.KudaLepas;
    }

    void Start() {
        EnsureAnimalVisuals();
        // Konfigurasi dinamis antara Rumah vs Pohon demi balancing game
        if (isTree) {
            burnOutTimer = 15f;           // Pohon lebih rentan dan cepat gosong
            spreadRadius = 16f;            // Merembet lebih jauh di area hutan
            spreadInterval = 20f;          // Diperlambat agar tidak merembet terlalu cepat (sebelumnya 8f)
            spreadVariance = 6f;           // Rentang acak penyebaran pohon (14s - 26s)
            regenerationDuration = 20f;   // Waktu tumbuh kembali secara otomatis
        } else {
            spreadVariance = 10f;          // Rumah: 20s - 40s
        }

        currentBurnTimer = burnOutTimer; 
        currentSpreadInterval = Random.Range(Mathf.Max(1f, spreadInterval - spreadVariance), spreadInterval + spreadVariance);
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
                if (spreadTimer >= currentSpreadInterval) {
                    spreadTimer = 0f;
                    SpreadFireToNeighbors();
                    // Acak ulang interval untuk penyebaran berikutnya
                    currentSpreadInterval = Random.Range(Mathf.Max(1f, spreadInterval - spreadVariance), spreadInterval + spreadVariance);
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
        
        // Tentukan waktu acak untuk penyebaran pertama kali terbakar
        currentSpreadInterval = Random.Range(Mathf.Max(1f, spreadInterval - spreadVariance), spreadInterval + spreadVariance);
        spreadTimer = 0f;

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
    
    private void EnsureAnimalVisuals()
    {
        if (snakeVisual == null)
        {
            Transform child = transform.Find("SnakeVisual");
            if (child != null)
            {
                snakeVisual = child.gameObject;
            }
            else
            {
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                placeholder.name = "SnakeVisual";
                placeholder.transform.SetParent(this.transform);
                placeholder.transform.localPosition = new Vector3(-0.8f, 0.5f, -0.8f);
                placeholder.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
                var renderer = placeholder.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                }
                var col = placeholder.GetComponent<Collider>();
                if (col != null) Destroy(col);
                
                snakeVisual = placeholder;
                snakeVisual.SetActive(false);
            }
        }

        if (horseVisual == null)
        {
            Transform child = transform.Find("HorseVisual");
            if (child != null)
            {
                horseVisual = child.gameObject;
            }
            else
            {
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = "HorseVisual";
                placeholder.transform.SetParent(this.transform);
                placeholder.transform.localPosition = new Vector3(0.8f, 0.5f, 0.8f);
                placeholder.transform.localScale = new Vector3(0.6f, 0.6f, 0.9f);
                var renderer = placeholder.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.5f, 0.25f, 0.05f); // Brown
                }
                var col = placeholder.GetComponent<Collider>();
                if (col != null) Destroy(col);

                horseVisual = placeholder;
                horseVisual.SetActive(false);
            }
        }
    }

    public void SetToAdaUlar() {
        if (TutorialManager.isTutorialActive) return; 

        currentStatus = HouseStatus.AdaUlar;
        fireHealth = 40f; 
        UpdateVisuals();
        Debug.Log($"<color=green>Warga melaporkan adanya ULAR di {gameObject.name}! Damkar harap segera mengamankan.</color>");
    }

    public void SetToKudaLepas() {
        if (TutorialManager.isTutorialActive) return; 

        currentStatus = HouseStatus.KudaLepas;
        fireHealth = 70f; 
        UpdateVisuals();
        Debug.Log($"<color=green>Warga melaporkan adanya KUDA LEPAS di sekitar {gameObject.name}! Damkar harap segera mengamankan.</color>");
    }

    public void HandleAnimalRescue(float p) {
        if (TutorialManager.isTutorialActive) return; 

        if (currentStatus == HouseStatus.AdaUlar || currentStatus == HouseStatus.KudaLepas) {
            fireHealth -= p * Time.deltaTime;
            if (fireHealth <= 0) {
                string animalName = currentStatus == HouseStatus.AdaUlar ? "Ular" : "Kuda";
                float reward = currentStatus == HouseStatus.AdaUlar ? 60f : 100f;
                
                SetToAman();
                Debug.Log($"<color=green>Penyelamatan Selesai! {animalName} berhasil diamankan.</color>");
                
                if (EconomyManager.instance != null) {
                    EconomyManager.instance.AddMoney(reward);
                    EconomyManager.instance.OnMissionComplete(1, 0); 
                }
            }
        }
    }

    public void UpdateVisuals() {
        if (fireEffect != null) fireEffect.SetActive(false);
        if (meshNormal != null) meshNormal.SetActive(false);
        if (meshPuing != null) meshPuing.SetActive(false);
        if (snakeVisual != null) snakeVisual.SetActive(false);
        if (horseVisual != null) horseVisual.SetActive(false);

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

            case HouseStatus.AdaUlar:
                if (meshNormal != null) meshNormal.SetActive(true);
                if (snakeVisual != null) snakeVisual.SetActive(true);
                break;

            case HouseStatus.KudaLepas:
                if (meshNormal != null) meshNormal.SetActive(true);
                if (horseVisual != null) horseVisual.SetActive(true);
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