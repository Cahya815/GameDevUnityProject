using UnityEngine;

public class Flammable : MonoBehaviour
{
    [Header("Timer Settings")]
    public float burnOutTimer = 30f; // Waktu total sampai jadi puing
    private float currentBurnTimer;

    [Header("Status")]
    public HouseStatus currentStatus = HouseStatus.Aman;
    public bool isBurning;
    public float fireHealth = 100f;

    [Header("Visuals")]
    public GameObject fireEffect;
    public GameObject meshNormal;
    public GameObject meshPuing;

    void Start() {
        currentBurnTimer = burnOutTimer; // Isi timer saat mulai
        UpdateVisuals();
    }

    void Update() {
        // Jika tutorial aktif, script ini akan diam total dan tidak menghitung timer
        if (TutorialManager.isTutorialActive) {
            isBurning = false;
            return;
        }

        isBurning = (currentStatus == HouseStatus.Terbakar);

        if (currentStatus == HouseStatus.Terbakar) {
            currentBurnTimer -= Time.deltaTime; 

            if (currentBurnTimer <= 0) {
                currentBurnTimer = 0; 
                SetToPuing();
            }
        }
    }

    // Fungsi yang dicari LeverDirector
    public void SetToTerbakar() {
        if (TutorialManager.isTutorialActive) return; // ABAIKAN TOTAL selama tutorial

        currentStatus = HouseStatus.Terbakar;
        fireHealth = 10f;
        currentBurnTimer = burnOutTimer; // <--- WAJIB: Isi ulang bensin timernya di sini!
        UpdateVisuals();
    }

    public void Extinguish(float p) {
        if (TutorialManager.isTutorialActive) return; // ABAIKAN TOTAL selama tutorial

        if (currentStatus == HouseStatus.Terbakar) {
            fireHealth -= p * Time.deltaTime;
            if (fireHealth <= 0) {
                SetToAman();
                // COLOKAN: Beritahu backend/economy satu api berhasil padam
                if (EconomyManager.instance != null) {
                    EconomyManager.instance.OnMissionComplete(1, 0); // 1 api padam
                }
            }
        }
    }

    public void CleanRubble(float s) {
        if (TutorialManager.isTutorialActive) return; // ABAIKAN TOTAL selama tutorial

        if (currentStatus == HouseStatus.Puing) {
            SetToAman();
            Debug.Log("<color=orange>Puing dibersihkan!</color>");
            
            // COLOKAN: Berikan uang/skor karena sudah membersihkan puing
            if (EconomyManager.instance != null) {
                EconomyManager.instance.AddMoney(50f); // Contoh: dapat $50
            }
        }
    }

    public void SetToAman() {
        if (TutorialManager.isTutorialActive) return; // ABAIKAN TOTAL selama tutorial

        currentStatus = HouseStatus.Aman;
        fireHealth = 0;
        UpdateVisuals();
    }
    

    public void UpdateVisuals() {
        // Matikan SEMUA dulu biar bersih
        if (fireEffect != null) fireEffect.SetActive(false);
        if (meshNormal != null) meshNormal.SetActive(false);
        if (meshPuing != null) meshPuing.SetActive(false);

        // Jika tutorial aktif, paksa tampilkan rumah normal saja dan sembunyikan api
        if (TutorialManager.isTutorialActive) {
            if (meshNormal != null) meshNormal.SetActive(true);
            return; // Berhenti di sini
        }

        // Nyalakan yang HANYA dibutuhkan sesuai status
        switch (currentStatus) {
            case HouseStatus.Aman:
                if (meshNormal != null) meshNormal.SetActive(true);
                break;

            case HouseStatus.Terbakar:
                if (meshNormal != null) meshNormal.SetActive(true);
                if (fireEffect != null) fireEffect.SetActive(true);
                break;

            case HouseStatus.Puing:
                if (meshPuing != null) meshPuing.SetActive(true);
                break;
        }
    }

    public void SetToPuing() {
        if (TutorialManager.isTutorialActive) return; // ABAIKAN TOTAL selama tutorial

        currentStatus = HouseStatus.Puing;
        fireHealth = 0;
        UpdateVisuals();
        Debug.Log("<color=red>Rumah Hancur Menjadi Puing!</color>");
    }
}