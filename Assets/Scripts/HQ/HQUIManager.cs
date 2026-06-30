using UnityEngine;
using UnityEngine.UI;
using TMPro; // Jika pakai TextMeshPro

public class HQUIManager : MonoBehaviour
{
    public static HQUIManager instance;

    public GameObject menuPanel;
    public TextMeshProUGUI infoText;
    private HQController currentHQ;

    void Awake() { instance = this; menuPanel.SetActive(false); }

    public void OpenMenu(HQController hq)
{
    currentHQ = hq;
    menuPanel.SetActive(true);
    UpdateUI();

    // LAPOR KE TUTORIAL: "Woi, menu sudah dibuka nih!"
    // Pakai FindFirstObjectByType karena lo pakai Unity versi terbaru
    TutorialManager tutor = Object.FindFirstObjectByType<TutorialManager>();
    if (tutor != null && tutor.step == 1) // Sesuaikan angka step-nya
    {
        tutor.NextStep();
    }
}

    public void UpdateUI()
    {
        string disasterStatus = "";
        UnitManager um = Object.FindFirstObjectByType<UnitManager>();
        if (um != null)
        {
            disasterStatus = um.isDisasterUnitUnlocked 
                ? "\n<color=green>Disaster Unit: Unlocked</color>" 
                : $"\n<color=yellow>Disaster Unit: Locked (Cost: ${um.disasterUnlockCost})</color>";
        }
        infoText.text = $"{currentHQ.buildingName}\nLevel: {currentHQ.hqLevel}\nHP: {currentHQ.health}{disasterStatus}";
    }

    public void OnUpgradeClicked()
    {
        currentHQ.UpgradeBuilding();
        UpdateUI(); // Refresh angka di layar
    }

    public void OnUnlockDisasterClicked()
    {
        UnitManager um = Object.FindFirstObjectByType<UnitManager>();
        if (um != null)
        {
            if (um.TryUnlockDisasterUnit())
            {
                UpdateUI();
            }
        }
    }

    public void CloseMenu() => menuPanel.SetActive(false);
}
