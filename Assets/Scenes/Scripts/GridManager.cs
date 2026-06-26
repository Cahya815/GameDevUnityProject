using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject fireTruckPrefab;
    public Transform spawnPoint;
    public float unitCost = 200f;

    public void BuyUnit()
    {
        if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(unitCost))
        {
            GameObject newUnit = Instantiate(fireTruckPrefab, spawnPoint.position, Quaternion.identity);
            
            // Ambil identitasnya
            UnitIdentity id = newUnit.GetComponent<UnitIdentity>();
            
            // DAFTAR KE UNIT MANAGER menggunakan fungsi baru yang direkomendasikan Unity
            UnitManager um = FindFirstObjectByType<UnitManager>();
            if (um != null && id != null)
            {
                um.allUnits.Add(id);
                Debug.Log("Unit baru lahir dan terdaftar di list!");
            }
        }
    }

    void Update()
    {
        // SATU GERBANG INPUT: Klik Kiri Mouse
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // 1. CEK HQ TERLEBIH DAHULU
                if (hit.collider.TryGetComponent(out HQController hq))
                {
                    hq.OpenMenu();
                    return; // Keluar dari fungsi agar tidak mengeksekusi perintah jalan di bawah
                }

                // 2. JALANKAN LOGIKA PERINTAH UNIT
                UnitManager um = FindFirstObjectByType<UnitManager>();

                if (um != null && um.selectedUnit != null)
                {
                    // FIX AKURASI: Cari komponen Flammable dari objek yang tertembak, 
                    // naik ke parent, atau bahkan scan seluruh komponen di strukturnya.
                    Flammable targetFlammable = hit.collider.GetComponent<Flammable>();
                    if (targetFlammable == null)
                    {
                        targetFlammable = hit.collider.GetComponentInParent<Flammable>();
                    }

                    // Perintahkan NavMeshAgent jalan
                    var agent = um.selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null) 
                    {
                        if (targetFlammable != null)
                        {
                            // Hitung parkir dinamis sesuai arah kedatangan unit saat ini agar tidak selalu parkir di belakang
                            Vector3 targetPos = targetFlammable.transform.position;
                            Vector3 currentPos = agent.transform.position;
                            Vector3 direction = (currentPos - targetPos).normalized;
                            direction.y = 0;
                            if (direction == Vector3.zero) direction = agent.transform.forward;

                            float offsetDist = 3.5f; // Jarak aman di luar collider rumah
                            Vector3 targetDestination = targetPos + direction.normalized * offsetDist;

                            if (UnityEngine.AI.NavMesh.SamplePosition(targetDestination, out UnityEngine.AI.NavMeshHit navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                            {
                                agent.SetDestination(navHit.position);
                            }
                            else
                            {
                                agent.SetDestination(targetPos);
                            }
                        }
                        else
                        {
                            // Jika klik tanah kosong, arahkan ke titik klik
                            agent.SetDestination(hit.point);
                        }
                    }

                    // Sinkronisasi target utama unit
                    um.selectedUnit.targetObject = targetFlammable;
                    um.selectedUnit.isManualControlled = true;

                    // Sinkronisasi target ke komponen spesifik mobil
                    if (um.selectedUnit.TryGetComponent(out FireTruck ft)) 
                    {
                        if (targetFlammable != null)
                            ft.SetTarget(targetFlammable);
                        else
                            ft.SetTarget(hit.point);
                    }
                    
                    if (um.selectedUnit.TryGetComponent(out DisasterUnit du)) 
                    {
                        if (targetFlammable != null)
                            du.SetTarget(targetFlammable);
                        else
                            du.SetTarget(hit.point);
                    }

                    // LOGGING UNTUK DIAGNOSIS INFO
                    if (targetFlammable != null)
                    {
                        string canBeExtinguished = " (Status: " + targetFlammable.currentStatus.ToString() + ")";
                        Debug.LogWarning("BERHASIL INTERAKSI! Unit " + um.selectedUnit.name + " ditargetkan ke RUMAH: " + targetFlammable.gameObject.name + canBeExtinguished);
                    }
                    else
                    {
                        Debug.Log("Unit " + um.selectedUnit.name + " diperintahkan ke TANAH KOSONG: " + hit.point);
                    }
                }
            }
        }
    }
}