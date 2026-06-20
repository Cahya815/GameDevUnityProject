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

    [Header("Crew Settings")]
    public GameObject crewPrefab;
    private FirefighterCrew activeCrew;
    private bool isCrewDeployed = false;

    private Flammable targetFire;
    private NavMeshAgent agent;
    private bool hasLoggedSpray = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Cari apakah sudah ada crew di dalam mobil (sebagai child object)
        activeCrew = GetComponentInChildren<FirefighterCrew>(true);
        if (activeCrew != null)
        {
            crewPrefab = activeCrew.gameObject;
            activeCrew.gameObject.SetActive(false); // Pastikan nonaktif di awal game
        }
        else if (crewPrefab == null)
        {
            // Coba cari di folder Resources jika ada
            crewPrefab = Resources.Load<GameObject>("FirefighterCrew");
            
            if (crewPrefab == null)
            {
                Debug.LogError("<color=red><b>[ERROR] Crew Prefab Hilang di Mobil Pemadam!</b></color>\n" +
                               "Ini terjadi karena Anda memasang prefab crew pada mobil yang ada di <b>Hierarchy (Scene)</b>, sedangkan mobil baru di-spawn dari <b>Prefab asli</b> di folder Project.\n\n" +
                               "<b>CARA MEMPERBAIKI:</b>\n" +
                               "1. Klik folder <b>Assets</b> di tab <b>Project</b> (bukan Hierarchy).\n" +
                               "2. Temukan file prefab <b>FireTruck</b> (mobil damkar) Anda di sana, lalu klik 2x untuk membuka Prefab.\n" +
                               "3. Pada Inspector prefab <b>FireTruck</b> tersebut, seret prefab <b>FirefighterCrew</b> Anda ke kolom <b>Crew Prefab</b>.\n" +
                               "4. Jalankan kembali game Anda! Sekarang crew tidak akan missing lagi.");
            }
        }
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
                // Jika sedang ada crew, suruh crew pulang dulu sebelum ganti target
                if (isCrewDeployed && activeCrew != null)
                {
                    activeCrew.ReturnToTruck();
                }
                targetFire = newTarget;
                hasLoggedSpray = false;
            }
        }
        else
        {
            if (isCrewDeployed && activeCrew != null)
            {
                activeCrew.ReturnToTruck();
            }
            targetFire = null;
            hasLoggedSpray = false;
        }
    }

    public void SetTarget(Flammable target)
    {
        if (targetFire != target)
        {
            if (isCrewDeployed && activeCrew != null)
            {
                activeCrew.ReturnToTruck();
            }
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

    private void DeployCrew()
    {
        if (isCrewDeployed || crewPrefab == null || targetFire == null) return;

        GameObject crewObj = null;

        // Jika crew sudah ada sebagai child, kita tinggal aktifkan dan posisikan ulang
        if (activeCrew != null)
        {
            crewObj = activeCrew.gameObject;
            crewObj.transform.position = transform.position + transform.forward * 1.5f;
            crewObj.SetActive(true);
        }
        else
        {
            // Jika tidak ada child, kita spawn dari prefab
            crewObj = Instantiate(crewPrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);
            activeCrew = crewObj.GetComponent<FirefighterCrew>();
        }

        if (activeCrew != null)
        {
            isCrewDeployed = true;
            
            // Ambil power dari UnitIdentity jika ada, kalau tidak pakai extinguishPower
            float power = extinguishPower;
            if (TryGetComponent(out UnitIdentity ui))
            {
                power = ui.power;
            }

            activeCrew.Initialize(this, targetFire, power);
            
            // Setup NavMeshAgent crew
            NavMeshAgent crewAgent = crewObj.GetComponent<NavMeshAgent>();
            if (crewAgent != null)
            {
                // -1 mengaktifkan semua area mask agar crew bisa jalan di grass dan dense forest
                crewAgent.areaMask = -1;
                
                // Pindahkan agent ke posisi warp yang benar agar NavMesh tidak meleset
                crewAgent.Warp(transform.position + transform.forward * 1.5f);
            }
        }
    }

    public void OnCrewReturned()
    {
        isCrewDeployed = false;
        targetFire = null;
        
        if (activeCrew != null)
        {
            activeCrew.gameObject.SetActive(false);
        }
        
        if (TryGetComponent(out UnitIdentity ui))
        {
            ui.targetObject = null;
            ui.isManualControlled = false;
        }
        
        if (agent != null)
        {
            agent.isStopped = false;
        }
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
                if (activeCrew != null)
                {
                    activeCrew.gameObject.SetActive(false);
                    isCrewDeployed = false;
                }
                return;
            }
        }

        // Jika crew sedang dideploy, hentikan truk dan biarkan crew bekerja
        if (isCrewDeployed)
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Jika air habis (hanya untuk kebakaran) atau target tidak aktif lagi, suruh crew kembali
            bool shouldReturn = targetFire == null || !targetFire.IsActiveFirefighterEmergency();
            if (targetFire != null && targetFire.currentStatus == HouseStatus.Terbakar && currentWater <= 0f)
            {
                shouldReturn = true;
            }

            if (shouldReturn)
            {
                if (activeCrew != null)
                {
                    activeCrew.ReturnToTruck();
                }
            }
            return;
        }

        // 2. Logika pergerakan truk ke arah target terbakar/darurat dan mendeploy crew
        if (targetFire != null)
        {
            if (targetFire.IsActiveFirefighterEmergency())
            {
                if (agent != null)
                {
                    if (agent.destination != targetFire.transform.position)
                    {
                        agent.SetDestination(targetFire.transform.position);
                    }
                    
                    // Cek apakah truk sudah sampai/parkir di jalan terdekat ke target
                    float distToTarget = Vector3.Distance(transform.position, targetFire.transform.position);
                    bool isNearTarget = distToTarget <= 25f;

                    bool isTruckParked = !agent.pathPending && 
                                         (agent.remainingDistance <= agent.stoppingDistance + 0.5f || (isNearTarget && agent.velocity.sqrMagnitude < 0.05f));

                    if (isTruckParked)
                    {
                        agent.isStopped = true;
                        DeployCrew();
                    }
                    else
                    {
                        agent.isStopped = false;
                    }
                }
            }
            else
            {
                targetFire = null;
                if (agent != null) agent.isStopped = false;
            }
        }
        else
        {
            if (agent != null && ui != null && !ui.isManualControlled && ui.targetObject == null)
            {
                // Biarkan agent bergerak ke arah rumah (HQ)
            }
        }
    }
}