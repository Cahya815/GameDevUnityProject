using UnityEngine;
using UnityEngine.AI;

public class FireTruck : MonoBehaviour
{
    public float extinguishPower = 20f;
    public float stopDistance = 5f;
    
    private Flammable targetFire;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Nama fungsi diganti jadi SetTarget agar klop dengan GridManager!
    public void SetTarget(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f))
        {
            targetFire = hit.collider.GetComponentInParent<Flammable>();
        }
    }

    void Update()
    {
        // Hanya logika menyemprot jika sudah dekat dengan target rumah terbakar
        if (targetFire != null && targetFire.currentStatus == HouseStatus.Terbakar)
        {
            float distance = Vector3.Distance(transform.position, targetFire.transform.position);

            if (distance <= stopDistance)
            {
                agent.isStopped = true; 
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
            if (agent != null) agent.isStopped = false;
        }
    }
}