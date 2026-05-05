using UnityEngine;
using UnityEngine.AI;

public class GridManager : MonoBehaviour
{
    public LayerMask clickLayer; // Pastikan ini 'Everything' atau 'Default'
    public FireTruck activeUnit; // Ganti ke tipe FireTruck agar langsung kenal script-nya

    public GameObject fireTruckPrefab;
    public Transform spawnPoint;
    public float unitCost = 200f;

    public void BuyUnit()
{
    if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(unitCost))
    {
        GameObject newUnit = Instantiate(fireTruckPrefab, spawnPoint.position, Quaternion.identity);
        
        // AMBIL SCRIPT-NYA
        UnitIdentity id = newUnit.GetComponent<UnitIdentity>();
        
        // MASUKIN KE DAFTAR UnitManager biar bisa dipilih angka 1, 2, 3
        FindObjectOfType<UnitManager>().allUnits.Add(id); 
        
        Debug.Log("Unit baru lahir dan terdaftar di list!");
    }
}

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (activeUnit != null)
                {
                    // INI KUNCI PERBAIKANNYA:
                    // Kita kirim koordinatnya (Vector3), bukan script rumahnya
                    activeUnit.SetNewTarget(hit.point);
                    
                    Debug.Log("Unit diperintahkan ke: " + hit.point);
                }
            }
        }
    }
}