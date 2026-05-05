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

    

    void Update()
    {
        // Cari puing terdekat jika sedang nganggur update tes
        if (targetRubble == null)
        {
            FindNearestRubble();
        }
        else
        {
            float distance = Vector3.Distance(transform.position, targetRubble.transform.position);
            if (distance <= stoppingDistance)
            {
                CleanProcess();
            }
        }
    }

    void FindNearestRubble()
    {
        Flammable[] allHouses = FindObjectsOfType<Flammable>();
        float shortestDistance = Mathf.Infinity;

        foreach (Flammable house in allHouses)
        {
            if (house.currentStatus == HouseStatus.Puing)
            {
                float dist = Vector3.Distance(transform.position, house.transform.position);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    targetRubble = house;
                    agent.SetDestination(targetRubble.transform.position);
                }
            }
        }
    }

    void CleanProcess()
    {
        if (targetRubble != null)
        {
            // Kita asumsikan ada variabel baru di Flammable untuk proses pembersihan
            // Atau kita langsung panggil fungsi SetToAman
            targetRubble.SetToAman(); 
            targetRubble = null;
            Debug.Log("<color=orange>Puing Berhasil Dibersihkan!</color>");
        }
    }

    
}