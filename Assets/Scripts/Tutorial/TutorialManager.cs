using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    
    private int step = 0;

    void Start() {
        ShowStep();
    }

    public void ShowStep() {
        tutorialPanel.SetActive(true);
        switch (step) {
            case 0:
                tutorialText.text = "Selamat datang, Komandan! Ini adalah markas HQ lo.";
                // Highlight HQ
                break;
            case 1:
                tutorialText.text = "Klik HQ untuk melihat status dan menu upgrade.";
                nextButton.gameObject.SetActive(false); // Sembunyikan tombol 'Next' biar pemain dipaksa klik HQ
                break;
            case 2:
                tutorialText.text = "Bagus! Sekarang coba tekan tombol UPGRADE.";
                break;
            default:
                EndTutorial();
                break;
        }
    }

    public void NextStep() {
        step++;
        ShowStep();
    }

    void EndTutorial() {
        tutorialPanel.SetActive(false);
        Debug.Log("Tutorial Selesai!");
    }
}