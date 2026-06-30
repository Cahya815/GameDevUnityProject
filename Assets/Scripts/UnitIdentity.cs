using UnityEngine;
using UnityEngine.AI;

public class UnitIdentity : MonoBehaviour 
{
    [Header("Settings")]
    public UnitType jenisUnit;
    public float power = 50f;

    [Header("Crew Training Settings")]
    public int crewLevel = 1;
    public bool isTraining = false;
    public float trainingTimer = 0f;
    public float trainingDuration = 0f;

    [Header("Engine Condition Settings")]
    public float engineCondition = 100f; // 0 sampai 100
    public bool isStalled = false; // mogok
    public float engineDegradationRate = 0.5f; // berkurang 0.5% per detik saat aktif bekerja/jalan

    [Header("State")]
    public Flammable targetObject; 
    public bool isManualControlled = false;
    
    public NavMeshAgent agent; // Sekarang UnitManager bisa ngasih perintah
    [HideInInspector] public Vector3 spawnPosition; // Koordinat rumah asli (HQ)
    private bool isReturningHome = false;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        // CATAT POSISI AWAL saat baru spawn/start
        spawnPosition = transform.position;
    }

    void Update() {
        // Cek jika sedang mogok
        if (isStalled) {
            if (agent != null) {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;
        }

        // Cek jika sedang latihan
        if (isTraining) {
            if (agent != null) {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            trainingTimer -= Time.deltaTime;
            if (trainingTimer <= 0f) {
                CompleteTraining();
            }
            return;
        }

        // Degradasi kondisi mesin saat bergerak atau memiliki target (sedang bekerja)
        if ((agent != null && agent.velocity.magnitude > 0.1f) || targetObject != null) {
            engineCondition -= engineDegradationRate * Time.deltaTime;
            if (engineCondition <= 0f) {
                engineCondition = 0f;
                isStalled = true;
                targetObject = null;
                isManualControlled = false;
                if (TryGetComponent(out FireTruck ft)) ft.SetTarget(null);
                if (TryGetComponent(out DisasterUnit du)) du.SetTarget(null);
                Debug.LogError($"<color=red>MOGOK! {gameObject.name} mesinnya mogok karena tidak dirawat. Butuh rehabilitasi mesin!</color>");
            }
        }

        // Cek apakah target sudah selesai dikerjakan atau sudah tidak valid
        if (targetObject != null) {
            bool isFinished = false;

            if (targetObject.currentStatus == HouseStatus.Aman) {
                isFinished = true;
            }
            else if (jenisUnit == UnitType.Firefighter && !targetObject.IsActiveFirefighterEmergency()) {
                isFinished = true;
            }
            else if (jenisUnit == UnitType.DisasterControl && targetObject.currentStatus == HouseStatus.Puing && targetObject.isTree) {
                isFinished = true;
            }

            if (isFinished) {
                targetObject = null;
                isManualControlled = false;
                
                // Sinkronisasi target ke komponen spesifik mobil
                if (TryGetComponent(out FireTruck ft)) {
                    ft.SetTarget(null);
                }
                if (TryGetComponent(out DisasterUnit du)) {
                    du.SetTarget(null);
                }
            }
        }

        // 1. LOGIKA BALIK KE RUMAH (Hanya jika tidak dikontrol & tidak punya kerjaan)
        if (!isManualControlled && targetObject == null) {
            float distToHome = Vector3.Distance(transform.position, spawnPosition);
            
            if (distToHome > 1.5f && !isReturningHome) {
                ReturnToHome();
            } 
            else if (distToHome <= 1.5f) {
                // Sampai di rumah, stop total (Idle)
                isReturningHome = false;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        // 2. LOGIKA KERJA
        if (targetObject != null) {
            isReturningHome = false;

            if (jenisUnit == UnitType.Firefighter) {
                // Truk pemadam digerakkan ke arah target, tapi tidak memadamkan langsung (di-handle oleh FireTruck dan Crew)
                if (!isManualControlled) {
                    agent.SetDestination(targetObject.transform.position);
                }
            } else {
                agent.isStopped = false;
                float dist = Vector3.Distance(transform.position, targetObject.transform.position);
                
                if (!isManualControlled) {
                    agent.SetDestination(targetObject.transform.position);
                }

                if (dist < 3.5f) {
                    DoWork();
                }
            }
        }
    }

    public void DoWork() {
        if (targetObject == null) return;

        if (jenisUnit == UnitType.Firefighter && targetObject.IsActiveFirefighterEmergency()) {
            if (targetObject.currentStatus == HouseStatus.Terbakar) {
                // Cek apakah punya komponen FireTruck dan airnya habis
                if (TryGetComponent(out FireTruck ft)) {
                    if (ft.currentWater <= 0) {
                        Debug.LogWarning($"{gameObject.name} kehabisan air! Tidak bisa memadamkan.");
                        // kok mencet 0 malah kepause ya
                        targetObject = null;
                        isManualControlled = false;
                        ft.SetTarget(null);
                        return;
                    }
                }
                targetObject.Extinguish(power);
            } else if (targetObject.currentStatus == HouseStatus.AdaUlar) {
                targetObject.HandleAnimalRescue(power);
            }
        } 
        else if (jenisUnit == UnitType.DisasterControl && targetObject.currentStatus == HouseStatus.Puing) {
            if (targetObject.isTree) {
                targetObject = null; // Pohon tidak perlu dibersihkan, lepas target
                isManualControlled = false;
                if (TryGetComponent(out DisasterUnit du)) {
                    du.SetTarget(null);
                }
            } else {
                targetObject.CleanRubble(power);
            }
        }

        if (targetObject != null && targetObject.currentStatus == HouseStatus.Aman) {
            targetObject = null; //balik HQ
            isManualControlled = false;
            
            if (TryGetComponent(out FireTruck ft)) {
                ft.SetTarget(null);
            }
            if (TryGetComponent(out DisasterUnit du)) {
                du.SetTarget(null);
            }
        }
    }

    public void ReturnToHome() {
        isReturningHome = true;
        if (agent != null) {
            agent.isStopped = false;
            agent.SetDestination(spawnPosition);
        }
    }

    public void StartTraining() {
        if (isTraining || isStalled) return;

        if (HQController.currentHQLevel < 2)
        {
            Debug.LogWarning($"<color=red>HQ Level {HQController.currentHQLevel} terlalu rendah untuk melatih crew! Dibutuhkan HQ Level 2.</color>");
            return;
        }

        // Jika HQ Level 4, tidak ada batasan level crew. Jika tidak, batasi sesuai HQ Level.
        int maxCrewLevel = (HQController.currentHQLevel >= 4) ? int.MaxValue : (HQController.currentHQLevel - 1);
        if (crewLevel >= maxCrewLevel && HQController.currentHQLevel < 4)
        {
            Debug.LogWarning($"<color=red>Crew {gameObject.name} sudah mencapai level maksimum ({maxCrewLevel}) untuk HQ Level {HQController.currentHQLevel}! Upgrade HQ untuk melatih lebih lanjut.</color>");
            return;
        }

        float cost = crewLevel * 100f; // Biaya naik seiring level
        float duration = crewLevel * 5f; // Waktu latihan naik

        if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(cost)) {
            isTraining = true;
            trainingDuration = duration;
            trainingTimer = duration;
            targetObject = null;
            isManualControlled = false;
            if (TryGetComponent(out FireTruck ft)) ft.SetTarget(null);
            if (TryGetComponent(out DisasterUnit du)) du.SetTarget(null);

            // Teleport ke HQ untuk latihan
            transform.position = spawnPosition;
            if (agent != null) {
                agent.Warp(spawnPosition);
                agent.isStopped = true;
            }

            Debug.Log($"<color=cyan>{gameObject.name} mulai Latihan Anggota (Level {crewLevel} -> {crewLevel + 1}). Biaya: ${cost}, Durasi: {duration}s</color>");
        } else {
            Debug.LogWarning("<color=yellow>Uang tidak cukup untuk Latihan Anggota!</color>");
        }
    }

    private void CompleteTraining() {
        isTraining = false;
        crewLevel++;
        power += 15f; // Upgrade kekuatan
        if (agent != null) {
            agent.speed += 0.5f; // Upgrade kecepatan jalan
            agent.isStopped = false;
        }
        Debug.Log($"<color=green>Latihan Selesai! Anggota {gameObject.name} naik ke Level {crewLevel}! Power & Speed meningkat.</color>");
    }

    public void RehabilitateEngine() {
        if (HQController.currentHQLevel < 3)
        {
            Debug.LogWarning($"<color=red>HQ Level {HQController.currentHQLevel} terlalu rendah untuk merehabilitasi mesin! Dibutuhkan HQ Level 3.</color>");
            return;
        }

        float cost = (100f - engineCondition) * 1f; // $1 per 1% damage
        if (cost < 5f) cost = 5f; // Minimal biaya $5

        if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(cost)) {
            engineCondition = 100f;
            isStalled = false;
            if (agent != null) agent.isStopped = false;
            Debug.Log($"<color=green>{gameObject.name} berhasil direhabilitasi seharga ${cost:F0}! Mesin kembali prima.</color>");
        } else {
            Debug.LogWarning("<color=yellow>Uang tidak cukup untuk rehabilitasi mesin!</color>");
        }
    }
}