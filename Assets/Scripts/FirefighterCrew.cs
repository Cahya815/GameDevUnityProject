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
    private bool wasInitialized = false;

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
        wasInitialized = true;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(targetFire.transform.position);
        }

        Debug.Log($"[Crew] Crew deployed from {parentTruck.gameObject.name} targeting {targetFire.gameObject.name} with power {extinguishPower}");
    }

    void Update()
    {
        // Hanya hancurkan jika crew sudah pernah diinisialisasi dan truknya hilang/hancur
        if (wasInitialized && parentTruck == null)
        {
            // Jika truk hancur atau hilang, hancurkan crew
            Destroy(gameObject);
            return;
        }

        // Jika belum diinisialisasi, jangan lakukan apa-apa
        if (!wasInitialized)
        {
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
        if (targetFire == null || !targetFire.IsActiveFirefighterEmergency())
        {
            // Target sudah aman atau hancur sebelum sampai, pulang ke truk
            ReturnToTruck();
            return;
        }

        if (agent != null)
        {
            float distance = Vector3.Distance(transform.position, targetFire.transform.position);
            
            // Toleransi jika crew tertahan collider fisik gedung atau sampai di ujung NavMesh
            bool hasArrived = distance <= stoppingDistance || 
                              (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f) ||
                              (distance <= stoppingDistance + 1.5f && agent.velocity.sqrMagnitude < 0.05f);

            if (hasArrived)
            {
                agent.isStopped = true;
                currentState = CrewState.Extinguishing;
                Debug.Log($"[Crew] Arrived at target {targetFire.gameObject.name}, starting rescue/extinguish action.");
            }
        }
    }

    private void HandleExtinguishing()
    {
        if (targetFire == null || !targetFire.IsActiveFirefighterEmergency())
        {
            // Selesai memadamkan atau target hancur
            Debug.Log("[Crew] Target extinguished/secured or destroyed. Returning to truck.");
            ReturnToTruck();
            return;
        }

        if (targetFire.currentStatus == HouseStatus.Terbakar)
        {
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
        else if (targetFire.currentStatus == HouseStatus.AdaUlar)
        {
            // Tidak butuh air untuk menangkap hewan
            targetFire.HandleAnimalRescue(extinguishPower);
        }
    }

    private void HandleReturningToTruck()
    {
        if (agent != null)
        {
            float distance = Vector3.Distance(transform.position, parentTruck.transform.position);
            bool hasArrived = distance <= 2f || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f);
            if (hasArrived)
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
