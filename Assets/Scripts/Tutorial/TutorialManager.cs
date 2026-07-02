using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public static bool isTutorialActive = false;
    public GameObject modeSelectionUI;
    
    public int step = 0;
    private bool tutorialCompleted = false;

    void Start()
    {
        // Tutorial tidak auto-start lagi.
        // Dipanggil dari ModeSelectionUI setelah player login.
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
                tutorialText.text = "Selamat datang, Komandan!\nIni adalah markas HQ lo.";
                break;
            case 1:
                tutorialText.text = "Klik HQ untuk melihat\nstatus dan menu upgrade.";
                nextButton.gameObject.SetActive(true);
                break;
            case 2:
                tutorialText.text = "Bagus! Sekarang coba tekan\ntombol UPGRADE.";
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
        // Tutorial selesai, aktifkan game
        tutorialPanel.SetActive(false);
        tutorialText.text = "";
        nextButton.gameObject.SetActive(false);
        step = 0;
        tutorialCompleted = true;

        // Simpan bahwa tutorial sudah selesai
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        Debug.Log("Tutorial Selesai!");

        // Tutorial selesai, aktifkan game
        isTutorialActive = false;
    }
}