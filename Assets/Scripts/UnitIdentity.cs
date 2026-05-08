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
            
            // JANGAN JALAN SENDIRI kalau manual. Biarkan GridManager yang gerakin.
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
            targetObject.CleanRubble(power);
        }

        if (targetObject.currentStatus == HouseStatus.Aman) {
            targetObject = null; // Selesai kerja, target dilepas -> otomatis balik rumah
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