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
        // Klik Kiri untuk memerintah unit yang sedang aktif berjalan
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Ambil UnitManager dengan fungsi baru agar tidak warning
                UnitManager um = FindFirstObjectByType<UnitManager>();

                if (um != null && um.selectedUnit != null)
                {
                    // 1. Perintahkan NavMeshAgent jalan ke titik klik
                    var agent = um.selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null) 
                    {
                        agent.SetDestination(hit.point);
                    }

                    // 2. Sinkronisasi target ke komponen spesifik mobil (Pakai SetTarget yang konsisten!)
                    if (um.selectedUnit.TryGetComponent(out FireTruck ft)) 
                    {
                        ft.SetTarget(hit.point);
                    }
                    if (um.selectedUnit.TryGetComponent(out DisasterUnit du)) 
                    {
                        du.SetTarget(hit.point);
                    }

                    Debug.Log("Unit " + um.selectedUnit.name + " diperintahkan ke: " + hit.point);
                }
            }
        }
    }
}