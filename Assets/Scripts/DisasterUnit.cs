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
        // Ambil script Identity buat cek status
        UnitIdentity identity = GetComponent<UnitIdentity>();

        // JANGAN JALAN SENDIRI kalau lagi dipilih player
        if (identity != null && identity.isManualControlled) {
            // Biarkan UnitManager yang ngatur NavMesh-nya
            return; 
        }

        // --- SISANYA LOGIKA OTOMATIS LO DI SINI ---
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