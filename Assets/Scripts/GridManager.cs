using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject fireTruckPrefab;
    public Transform spawnPoint;
    public float unitCost = 200f;

    public void BuyUnit()
    {
        // Panggil EconomyManager
        if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(unitCost))
        {
            GameObject newUnit = Instantiate(fireTruckPrefab, spawnPoint.position, Quaternion.identity);
            
            // Ambil identitasnya
            UnitIdentity id = newUnit.GetComponent<UnitIdentity>();
            
            // Daftarkan ke list UnitManager (Pusat Komando)
            // Ini supaya UnitManager tahu ada prajurit baru
            FindObjectOfType<UnitManager>().allUnits.Add(id); 
            
            Debug.Log("Unit baru dibeli dan didaftarkan ke Pusat Komando!");
        }
    }

    // UPDATE DIHAPUS TOTAL DARI SINI
    // Pindahkan semua logika klik kanan/kiri ke UnitManager
}