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

                // Di dalam Update() script Input lo:
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out HQController hq))
                {
                    hq.OpenMenu();
                }
            }
        }
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

                    // Cek apakah yang diklik adalah Flammable (rumah/pohon)
                    Flammable targetFlammable = hit.collider.GetComponent<Flammable>();
                    if (targetFlammable == null) targetFlammable = hit.collider.GetComponentInParent<Flammable>();

                    // Sinkronisasi target utama unit
                    um.selectedUnit.targetObject = targetFlammable;
                    
                    // Setel ke kontrol manual karena pemain baru saja memberi perintah manual baru
                    um.selectedUnit.isManualControlled = true;

                    // 2. Sinkronisasi target ke komponen spesifik mobil (Pakai SetTarget yang konsisten!)
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

                    Debug.Log("Unit " + um.selectedUnit.name + " diperintahkan ke: " + hit.point);
                }
            }
        }
    }
}