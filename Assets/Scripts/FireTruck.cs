using UnityEngine;
using UnityEngine.AI;

public class FireTruck : MonoBehaviour
{
    public float extinguishPower = 20f;
    public float stopDistance = 5f;
    
    [Header("Water Tank Settings")]
    public float maxWater = 100f;
    public float currentWater = 100f;
    public float waterConsumptionRate = 15f; // berkurang 15 unit per detik saat menyemprot
    public float waterRefillRate = 25f;       // terisi 25 unit per detik di dekat HQ/Hidran

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

    private bool CheckRefillSources()
    {
        // 1. Cek jarak ke HQ (spawnPosition dari UnitIdentity)
        if (TryGetComponent(out UnitIdentity id))
        {
            if (Vector3.Distance(transform.position, id.spawnPosition) <= 5f)
            {
                return true;
            }
        }

        // 2. Cek jarak ke Hydrant terdekat
        Hydrant[] hydrants = Object.FindObjectsByType<Hydrant>(FindObjectsSortMode.None);
        foreach (var hydrant in hydrants)
        {
            if (hydrant != null && Vector3.Distance(transform.position, hydrant.transform.position) <= 5f)
            {
                return true;
            }
        }

        return false;
    }

    void Update()
    {
        // 1. Logika isi ulang air jika dekat HQ atau Hidran
        bool isNearRefill = CheckRefillSources();
        if (isNearRefill)
        {
            if (currentWater < maxWater)
            {
                currentWater = Mathf.MoveTowards(currentWater, maxWater, waterRefillRate * Time.deltaTime);
            }
        }

        // Cek apakah sedang mogok atau latihan dari UnitIdentity
        if (TryGetComponent(out UnitIdentity ui))
        {
            if (ui.isStalled || ui.isTraining)
            {
                targetFire = null;
                return;
            }
        }

        // 2. Logika menyemprot jika dekat dengan target rumah terbakar
        if (targetFire != null)
        {
            if (targetFire.currentStatus == HouseStatus.Terbakar)
            {
                float distance = Vector3.Distance(transform.position, targetFire.transform.position);

                if (distance <= stopDistance)
                {
                    agent.isStopped = true; 

                    // Pastikan air masih ada sebelum menyemprot
                    if (currentWater > 0f)
                    {
                        targetFire.Extinguish(extinguishPower);
                        currentWater -= waterConsumptionRate * Time.deltaTime;
                        if (currentWater < 0) currentWater = 0;

                        if (!hasLoggedSpray)
                        {
                            Debug.Log($"Mulai menyemprot {targetFire.gameObject.name}. Sisa Air: {currentWater:F1}");
                            hasLoggedSpray = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Air habis! Harus isi ulang di HQ atau Hidran.");
                        // Lepas target agar mobil pulang ke HQ secara otomatis untuk isi ulang
                        targetFire = null;
                        hasLoggedSpray = false;
                        if (ui != null)
                        {
                            ui.targetObject = null;
                            ui.isManualControlled = false;
                        }
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