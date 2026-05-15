using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ModeSelectionUI : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public GameObject loginPanel;
    public GameObject modePanel;

    private void Start()
{
    // Pastikan AuthManager ada
    if (AuthManager.instance == null)
    {
        Debug.LogError("AuthManager tidak ditemukan!");
        return;
    }

    // Jangan aktifkan panel di sini, biarkan TutorialManager yang handle
    modePanel.SetActive(false);
    loginPanel.SetActive(false);
}

public void ShowModeSelection()
{
    modePanel.SetActive(true);
    loginPanel.SetActive(false);
}

    public void OnOnlineModeClicked()
    {
        string name = playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Username tidak boleh kosong!");
            return;
        }

        modePanel.SetActive(false);
        loginPanel.SetActive(true);
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
        SceneManager.LoadScene("GameScene");
    }

    public async void OnLoginClicked()
    {
        string name = playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Username tidak boleh kosong!");
            return;
        }

        await AuthManager.instance.LoginOnline(name, "password");
        SceneManager.LoadScene("GameScene");
    }

    public void OnBackClicked()
    {
        loginPanel.SetActive(false);
        modePanel.SetActive(true);
    }
}