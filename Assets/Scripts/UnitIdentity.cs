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
    public bool isManualControlled = false;
    
    private NavMeshAgent agent;
    private bool isReturningToHQ = false;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
    if (targetObject != null) {
        float dist = Vector3.Distance(transform.position, targetObject.transform.position);

        // HANYA gerak otomatis kalau TIDAK sedang dikendalikan manual
        if (!isManualControlled) {
            agent.SetDestination(targetObject.transform.position);
        }

        if (dist < 3.5f) {
            DoWork();
        }
    } 
    else if (!isManualControlled && !isReturningToHQ) {
        // Balik ke HQ cuma kalau bener-bener nganggur
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

        // Jika target sudah Aman, lepas target agar bisa balik ke HQ
        if (targetObject.currentStatus == HouseStatus.Aman) {
            targetObject = null;
        }
    }

    public void ReturnToHQ() {
        isReturningToHQ = true;
        if(agent != null && hqLocation != null) {
            agent.SetDestination(hqLocation.position);
        }
    }
}