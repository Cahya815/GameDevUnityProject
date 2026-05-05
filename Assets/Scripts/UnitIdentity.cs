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
        // 1. Logika Balik ke HQ
        if (!isManualControlled && targetObject == null && hqLocation != null && !isReturningToHQ) {
            // Cek jika sudah di HQ, jangan spam perintah
            if (Vector3.Distance(transform.position, hqLocation.position) > 2f) {
                ReturnToHQ();
            }
        }

        // 2. Logika Kerja
        if (targetObject != null) {
            isReturningToHQ = false;
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            
            // Perintahkan NavMesh ke target jika otomatis
            if (!isManualControlled) {
                agent.SetDestination(targetObject.transform.position);
            }

            if (dist < 3.5f) {
                DoWork(); // Fungsi ini yang dicari baris 27 tadi
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