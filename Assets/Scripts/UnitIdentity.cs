using UnityEngine;
using UnityEngine.AI;

public class UnitIdentity : MonoBehaviour 
{
    [Header("Settings")]
    public UnitType jenisUnit;
    public float power = 50f;

    [Header("State")]
    public Flammable targetObject; 
    public bool isManualControlled = false;
    
    public NavMeshAgent agent; // Sekarang UnitManager bisa ngasih perintah
    private Vector3 spawnPosition; // Koordinat rumah asli
    private bool isReturningHome = false;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        // CATAT POSISI AWAL saat baru spawn/start
        spawnPosition = transform.position;
    }

    void Update() {
        // Cek apakah target sudah selesai dikerjakan atau sudah tidak valid
        if (targetObject != null) {
            bool isFinished = false;

            if (targetObject.currentStatus == HouseStatus.Aman) {
                isFinished = true;
            }
            else if (jenisUnit == UnitType.Firefighter && targetObject.currentStatus == HouseStatus.Puing) {
                // Jika rumah/pohon terlanjur hangus terbakar menjadi puing, tidak ada api lagi untuk disemprot
                isFinished = true;
            }
            else if (jenisUnit == UnitType.DisasterControl && targetObject.currentStatus == HouseStatus.Puing && targetObject.isTree) {
                // Pohon gosong tidak perlu dibersihkan secara manual (ia regenerasi otomatis)
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
            agent.isStopped = false;

            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            
            // belum tau cara kerja dia bisa muncul toggle di unity kayaknya penting tapi gua gapaham samasekali njir beda banget sama html
            if (!isManualControlled) {
                agent.SetDestination(targetObject.transform.position);
            }

            if (dist < 3.5f) {
                DoWork();
            }
        }
    }

    public void DoWork() {
        if (targetObject == null) return;

        if (jenisUnit == UnitType.Firefighter && targetObject.currentStatus == HouseStatus.Terbakar) {
            targetObject.Extinguish(power);
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
}