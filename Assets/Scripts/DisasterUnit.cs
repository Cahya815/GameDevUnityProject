using UnityEngine;
using UnityEngine.AI;

public class DisasterUnit : MonoBehaviour
{
    public float cleanSpeed = 20f;
    public float stoppingDistance = 3f;
    
    private Flammable targetRubble;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Nama fungsi disamakan menjadi SetTarget!
    public void SetTarget(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 5, Vector3.down, out hit, 10f))
        {
            targetRubble = hit.collider.GetComponentInParent<Flammable>();
        }
    }

    void Update()
    {
        // NO AUTOMATIC RADAR. Hanya memproses pembersihan jika target sengaja diklik player
        if (targetRubble != null)
        {
            float distance = Vector3.Distance(transform.position, targetRubble.transform.position);
            if (distance <= stoppingDistance)
            {
                CleanProcess();
            }
        }
    }

    // Fungsi pencari puing otomatis sengaja kita singkirkan atau matikan agar tidak jalan sendiri
    // void FindNearestRubble()
    // {
    //     // Menggunakan FindObjectsByType yang baru agar tidak warning
    //     Flammable[] allHouses = FindObjectsByType<Flammable>(FindObjectsSortMode.None);
    //     float shortestDistance = Mathf.Infinity;

    //     foreach (Flammable house in allHouses)
    //     {
    //         if (house.currentStatus == HouseStatus.Puing)
    //         {
    //             float dist = Vector3.Distance(transform.position, house.transform.position);
    //             if (dist < shortestDistance)
    //             {
    //                 shortestDistance = dist;
    //                 targetRubble = house;
    //                 agent.SetDestination(targetRubble.transform.position);
    //             }
    //         }
    //     }
    // }

    void CleanProcess()
    {
        if (targetRubble != null)
        {
            targetRubble.SetToAman(); 
            targetRubble = null;
            Debug.Log("<color=orange>Puing Berhasil Dibersihkan!</color>");
        }
    }
}