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

    void Update()
    {
        // 1. Logika Pergerakan: Jika ada target, suruh agent ke sana
        if (targetPosition != Vector3.zero)
        {
            agent.SetDestination(targetPosition);
        }

        // 2. Logika Menyemprot: Cek jarak ke target rumah
        if (targetFire != null && targetFire.currentStatus == HouseStatus.Terbakar)
        {
            float distance = Vector3.Distance(transform.position, targetFire.transform.position);

            if (distance <= stopDistance)
            {
                // Berhenti di depan rumah agar tidak tabrakan
                agent.isStopped = true; 
                
                // Panggil fungsi padamkan di Parent
                targetFire.Extinguish(extinguishPower);
                Debug.Log("Menyemprot " + targetFire.gameObject.name);
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            // Jika rumah sudah padam (Aman) atau hancur (Puing), mobil berhenti kerja
            if (agent != null) agent.isStopped = false;
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