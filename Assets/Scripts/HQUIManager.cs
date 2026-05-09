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

    public void ShowMenu(HQController hq)
    {
        currentHQ = hq;
        menuPanel.SetActive(true);
        UpdateUI();
    }

    public void UpdateUI()
    {
        infoText.text = $"{currentHQ.buildingName}\nLevel: {currentHQ.level}\nHP: {currentHQ.health}";
    }

    public void OnUpgradeClicked()
    {
        currentHQ.UpgradeBuilding();
        UpdateUI(); // Refresh angka di layar
    }

    public void CloseMenu() => menuPanel.SetActive(false);
}
