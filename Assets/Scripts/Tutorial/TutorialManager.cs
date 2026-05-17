using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public static bool isTutorialActive = false;
    public GameObject modeSelectionUI;
    
    public int step = 0;
    private bool tutorialCompleted = false;

    void Start()
    {
        // Cek apakah tutorial sudah pernah selesai
        // if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        // {
        //     tutorialPanel.SetActive(false);
        //     tutorialCompleted = true;
        //     Debug.Log("Tutorial sudah selesai sebelumnya, skip tutorial");
        //     return;
        // }

        ShowStep();
    }

    public void StartTutorial()
    {
        if (tutorialCompleted) return;
        
        isTutorialActive = true;
        ShowStep();
    }

    public void NextStep()
    {
        step++;
        ShowStep();
    }

    public void ShowStep()
    {
        tutorialPanel.SetActive(true);
        switch (step)
        {
            case 0:
                tutorialText.text = "Selamat datang, Komandan! Ini adalah markas HQ lo.";
                break;
            case 1:
                tutorialText.text = "Klik HQ untuk melihat status dan menu upgrade.";
                nextButton.gameObject.SetActive(true);
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

    void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        tutorialText.text = "";
        nextButton.gameObject.SetActive(false);
        step = 0;
        tutorialCompleted = true;
        
        // Simpan bahwa tutorial sudah selesai
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        
        Debug.Log("Tutorial Selesai!");
        
        if (modeSelectionUI != null)
        {
            StartCoroutine(ShowModeSelectionDelayed());
        }
    }

    private System.Collections.IEnumerator ShowModeSelectionDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        
        ModeSelectionUI modeUI = modeSelectionUI.GetComponent<ModeSelectionUI>();
        if (modeUI != null)
        {
            modeUI.ShowModeSelection();
            Debug.Log("<color=green>Mode Selection Panel ditampilkan</color>");
        }
    }
}