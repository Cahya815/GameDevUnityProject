using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public static bool isTutorialActive = false;
    public GameObject modeSelectionUI; // Tambah referensi ke ModeSelectionUI
    
    public int step = 0;



     void Start() {
        ShowStep();
    }

    public void StartTutorial() {
        isTutorialActive = true;
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
                nextButton.gameObject.SetActive(true); // Sembunyikan tombol 'Next' biar pemain dipaksa klik HQ
                break;
            case 2:
                tutorialText.text = "Bagus! Sekarang coba tekan tombol UPGRADE.";
                nextButton.gameObject.SetActive(true);
                break;
             case 3:
                EndTutorial();
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
        // Tampilkan Mode Selection setelah tutorial selesai
        if (modeSelectionUI != null)
        {
            ModeSelectionUI modeUI = modeSelectionUI.GetComponent<ModeSelectionUI>();
            if (modeUI != null)
            {
                modeUI.ShowModeSelection();
            }
        }
        tutorialText.text = ""; // Kosongkan teks agar tidak nyangkut
        nextButton.gameObject.SetActive(false); 
        tutorialPanel.SetActive(false);
        Debug.Log("Tutorial Selesai!");
        tutorialPanel.SetActive(false);
        Debug.Log("Tutorial Selesai!");
         
         isTutorialActive = false;
        tutorialPanel.SetActive(false);
        step = 0;
        
        

    }
}