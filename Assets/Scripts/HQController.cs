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

    public void Interact() {
    HQUIManager.instance.OpenMenu(this);

    // Jika sedang tutorial step 1, lanjut ke step berikutnya
    if (FindObjectOfType<TutorialManager>() != null) {
        FindObjectOfType<TutorialManager>().NextStep();
    }
}

    public void UpgradeBuilding()
    {
        level++;
        health += 200f;
        Debug.Log("HQ Upgraded to Level: " + level);
    }
}