using UnityEngine;

public class HQController : MonoBehaviour
{
    public string buildingName = "Base HQ";
    public int level = 1;
    public float health = 500f;

    // Fungsi yang bakal dipanggil pas HQ diklik
    public void OpenMenu()
    {
        // Panggil Manager UI (kita buat di bawah)
        HQUIManager.instance.ShowMenu(this);
    }

    public void UpgradeBuilding()
    {
        level++;
        health += 200f;
        Debug.Log("HQ Upgraded to Level: " + level);
    }
}