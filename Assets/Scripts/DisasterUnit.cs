using UnityEngine;
using UnityEngine.AI;

public class DisasterUnit : MonoBehaviour
{
    public float cleanSpeed = 20f;
    public float stoppingDistance = 3f;

    private Flammable targetRubble;
    private NavMeshAgent agent;
    private Vector3 targetPosition = Vector3.zero; // Tambahkan deklarasi variabel ini

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

   void Update() {
    UnitIdentity identity = GetComponent<UnitIdentity>();

    // Jangan jalan sendiri jika unit idle
    if (identity != null && (identity.isIdle || !identity.isManualControlled)) {
        agent.isStopped = true; // Pastikan NavMeshAgent berhenti
        return; 
    }

    // Logika lainnya (jika ada target)
    if (targetPosition != Vector3.zero) {
        agent.SetDestination(targetPosition);
    }
}
    void FindNearestRubble()
    {
        // Logika untuk mencari puing terdekat
    }

    void CleanProcess()
    {
        if (targetRubble != null)
        {
            // Logika untuk membersihkan puing
        }
    }

    // Fungsi untuk mengatur target baru
    public void SetNewTarget(Vector3 position)
    {
        targetPosition = position;

        // Trik agar tahu script Flammable-nya:
        targetRubble = null; // Reset target puing
    }
}