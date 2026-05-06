using UnityEngine;
using UnityEngine.AI;

public class UnitIdentity : MonoBehaviour 
{
    [Header("Settings")]
    public UnitType jenisUnit;
    public Transform hqLocation; 
    public float power = 50f;

    [Header("State")]
    public Flammable targetObject; // Variabel ini yang dicari UnitManager
    public bool isManualControlled = false; // Hanya true jika unit dipilih
    public bool isIdle = true; // Tambahkan status idle untuk unit
    
    private NavMeshAgent agent;
    private bool isReturningToHQ = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        // Jika unit tidak dipilih (idle), jangan lakukan apa-apa
        if (isIdle) {
            agent.isStopped = true; // Pastikan NavMeshAgent berhenti
            return;
        }

        // Jika ada target, unit akan bergerak ke target
        if (targetObject != null) {
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);

            // Hanya gerak otomatis jika tidak dikendalikan manual
            if (!isManualControlled) {
                agent.SetDestination(targetObject.transform.position);
            }

            if (dist < 3.5f) {
                DoWork();
            }
        } 
        else if (!isManualControlled && !isReturningToHQ) {
            // Balik ke HQ hanya jika idle dan tidak ada target
            ReturnToHQ();
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

        // Jika target sudah aman, lepas target agar unit idle
        if (targetObject.currentStatus == HouseStatus.Aman) {
            targetObject = null;
        }
    }

    public void ReturnToHQ() {
        isReturningToHQ = true;
        if (agent != null && hqLocation != null) {
            agent.SetDestination(hqLocation.position);
        }
    }
}