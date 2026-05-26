using UnityEngine;
using UnityEngine.AI;

public class FireTruck : MonoBehaviour
{
    public float extinguishPower = 20f;
    public float stopDistance = 5f;
    
    private Flammable targetFire;
    private NavMeshAgent agent;
    private bool hasLoggedSpray = false;

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
            var newTarget = hit.collider.GetComponentInParent<Flammable>();
            if (targetFire != newTarget)
            {
                targetFire = newTarget;
                hasLoggedSpray = false;
            }
        }
        else
        {
            targetFire = null;
            hasLoggedSpray = false;
        }
    }

    public void SetTarget(Flammable target)
    {
        if (targetFire != target)
        {
            targetFire = target;
            hasLoggedSpray = false;
        }
    }

    void Update()
    {
        // Hanya logika menyemprot jika sudah dekat dengan target rumah terbakar
        if (targetFire != null)
        {
            if (targetFire.currentStatus == HouseStatus.Terbakar)
            {
                float distance = Vector3.Distance(transform.position, targetFire.transform.position);

                if (distance <= stopDistance)
                {
                    agent.isStopped = true; 
                    targetFire.Extinguish(extinguishPower);
                    if (!hasLoggedSpray)
                    {
                        Debug.Log("Mulai menyemprot " + targetFire.gameObject.name);
                        hasLoggedSpray = true;
                    }
                }
                else
                {
                    agent.isStopped = false;
                }
            }
            else if (targetFire.currentStatus == HouseStatus.Aman)
            {
                targetFire = null;
                if (agent != null) agent.isStopped = false;
            }
            else
            {
                if (agent != null) agent.isStopped = false;
            }
        }
        else
        {
            if (agent != null) agent.isStopped = false;
        }
    }
}