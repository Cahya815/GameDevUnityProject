using UnityEngine;
using UnityEngine.AI; // Wajib untuk gerakin mobil

public class FireTruck : MonoBehaviour
{
    public float extinguishPower = 20f;
    public float stopDistance = 5f; // Jarak tembak air
    
    private Vector3 targetPosition; // Simpan koordinat tujuan
    private Flammable targetFire;   // Simpan script rumah tujuan
    private NavMeshAgent agent;

    void Awake()
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

    // Fungsi yang dipanggil GridManager (Menerima Vector3 agar tidak error lagi)
    public void SetNewTarget(Vector3 position)
    {
        targetPosition = position;

        // Trik agar tahu script Flammable-nya: 
        // Kita cari script Flammable di lokasi yang kita klik
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f))
        {
            // Pakai GetComponentInParent karena script ada di bapaknya!
            targetFire = hit.collider.GetComponentInParent<Flammable>();
        }
    }
}