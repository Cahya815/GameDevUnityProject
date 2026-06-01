using UnityEngine;
using UnityEngine.AI;

public class FirefighterCrew : MonoBehaviour
{
    [Header("Crew Settings")]
    public float extinguishPower = 20f;
    public float stoppingDistance = 2f;
    public float walkSpeed = 3.5f;

    [Header("State")]
    [SerializeField] private CrewState currentState = CrewState.GoingToTarget;
    private FireTruck parentTruck;
    private Flammable targetFire;
    private NavMeshAgent agent;

    private enum CrewState
    {
        GoingToTarget,
        Extinguishing,
        ReturningToTruck
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = walkSpeed;
        }
    }

    public void Initialize(FireTruck truck, Flammable target, float power)
    {
        parentTruck = truck;
        targetFire = target;
        extinguishPower = power;
        currentState = CrewState.GoingToTarget;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(targetFire.transform.position);
        }

        Debug.Log($"[Crew] Crew deployed from {parentTruck.gameObject.name} targeting {targetFire.gameObject.name} with power {extinguishPower}");
    }

    void Update()
    {
        if (parentTruck == null)
        {
            // Jika truk hancur atau hilang, hancurkan crew
            Destroy(gameObject);
            return;
        }

        switch (currentState)
        {
            case CrewState.GoingToTarget:
                HandleGoingToTarget();
                break;

            case CrewState.Extinguishing:
                HandleExtinguishing();
                break;

            case CrewState.ReturningToTruck:
                HandleReturningToTruck();
                break;
        }
    }

    private void HandleGoingToTarget()
    {
        if (targetFire == null || targetFire.currentStatus != HouseStatus.Terbakar)
        {
            // Target sudah aman atau hancur sebelum sampai, pulang ke truk
            ReturnToTruck();
            return;
        }

        if (agent != null)
        {
            agent.SetDestination(targetFire.transform.position);

            float distance = Vector3.Distance(transform.position, targetFire.transform.position);
            if (distance <= stoppingDistance)
            {
                agent.isStopped = true;
                currentState = CrewState.Extinguishing;
                Debug.Log($"[Crew] Arrived at target {targetFire.gameObject.name}, starting to extinguish.");
            }
        }
    }

    private void HandleExtinguishing()
    {
        if (targetFire == null || targetFire.currentStatus != HouseStatus.Terbakar)
        {
            // Selesai memadamkan atau target hancur
            Debug.Log("[Crew] Target extinguished or destroyed. Returning to truck.");
            ReturnToTruck();
            return;
        }

        // Cek persediaan air truk
        if (parentTruck.currentWater <= 0f)
        {
            Debug.LogWarning("[Crew] Truck out of water! Returning to truck.");
            ReturnToTruck();
            return;
        }

        // Padamkan api
        targetFire.Extinguish(extinguishPower);

        // Konsumsi air dari tanki truk
        parentTruck.currentWater -= parentTruck.waterConsumptionRate * Time.deltaTime;
        if (parentTruck.currentWater < 0f)
        {
            parentTruck.currentWater = 0f;
        }
    }

    private void HandleReturningToTruck()
    {
        if (agent != null)
        {
            agent.SetDestination(parentTruck.transform.position);

            float distance = Vector3.Distance(transform.position, parentTruck.transform.position);
            if (distance <= 2f)
            {
                // Kembali masuk ke truk
                Debug.Log("[Crew] Returned to truck safely.");
                parentTruck.OnCrewReturned();
                gameObject.SetActive(false);
            }
        }
    }

    public void ReturnToTruck()
    {
        currentState = CrewState.ReturningToTruck;
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(parentTruck.transform.position);
        }
    }
}
