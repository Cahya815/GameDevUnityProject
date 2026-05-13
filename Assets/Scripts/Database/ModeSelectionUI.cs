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
        modePanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    public void OnOnlineModeClicked()
    {
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
}