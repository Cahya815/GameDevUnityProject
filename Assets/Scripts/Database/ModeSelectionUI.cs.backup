using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ModeSelectionUI : MonoBehaviour
{
    public static ModeSelectionUI instance;
    public TMP_InputField playerNameInput;
    public GameObject loginPanel;
    public GameObject modePanel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
{
    // Pastikan AuthManager ada
    if (AuthManager.instance == null)
    {
        Debug.LogError("AuthManager tidak ditemukan!");
        return;
    }

    // Langsung tampilkan panel login
    modePanel.SetActive(true);
    loginPanel.SetActive(true);

    // Sembunyikan panel tutorial saat login
    if (TutorialManager.instance != null && TutorialManager.instance.tutorialPanel != null)
    {
        TutorialManager.instance.tutorialPanel.SetActive(false);
    }
}

public void ShowModeSelection()
{
    modePanel.SetActive(true);
    loginPanel.SetActive(true);
}

    public void OnOnlineModeClicked()
    {
        TryStartOnlineLogin();
    }

    // Menyiapkan login online: buka browser Google OAuth, kelanjutan menunggu deep link
    private void TryStartOnlineLogin()
    {
        string name = playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Username tidak boleh kosong!");
            return;
        }

        AuthManager.instance.LoginOnline(name, null);
    }

    // Dipanggil AuthManager setelah token OAuth diterima via deep link
    public void FinishOnlineLogin()
    {
        GameDataManager.instance.InitializeDataHandler();

        // Mulai tutorial setelah login
        TutorialManager.instance.StartTutorial();

        modePanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    public void OnOfflineModeClicked()
{
    string name = playerNameInput.text;
    if (string.IsNullOrEmpty(name))
    {
        Debug.LogWarning("Nama pemain tidak boleh kosong!");
        return;
    }

    AuthManager.instance.SelectOfflineMode(name);
    GameDataManager.instance.InitializeDataHandler();
    
    // Mulai tutorial setelah login
    TutorialManager.instance.StartTutorial();
    
    // Sembunyikan panel sebelum load scene
    modePanel.SetActive(false);
    loginPanel.SetActive(false);
    
    
}

    public void OnLoginClicked()
    {
        TryStartOnlineLogin();
    }

    public void OnBackClicked()
    {
        loginPanel.SetActive(false);
        modePanel.SetActive(true);
    }
}