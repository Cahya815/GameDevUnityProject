using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ModeSelectionUI : MonoBehaviour
{
    public static ModeSelectionUI instance;

    [Header("Auth Panel")]
    public GameObject authPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public TextMeshProUGUI loginStatusText;

    [Header("Register Fields")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;
    public Button registerButton;
    public TextMeshProUGUI registerStatusText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (AuthManager.instance == null)
        {
            Debug.LogError("AuthManager tidak ditemukan!");
            return;
        }

        // Setup button listeners
        if (loginButton != null)
            loginButton.onClick.AddListener(() => _ = OnLoginClicked());

        if (registerButton != null)
            registerButton.onClick.AddListener(() => _ = OnRegisterClicked());

        // Show auth panel
        if (authPanel != null) authPanel.SetActive(true);

        // Sembunyikan panel tutorial saat login
        if (TutorialManager.instance != null && TutorialManager.instance.tutorialPanel != null)
        {
            TutorialManager.instance.tutorialPanel.SetActive(false);
        }
    }

    // ========== LOGIN ==========
    private async Task OnLoginClicked()
    {
        string email = loginEmailInput != null ? loginEmailInput.text.Trim() : "";
        string password = loginPasswordInput != null ? loginPasswordInput.text.Trim() : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLoginStatus("Email dan password harus diisi!", Color.red);
            return;
        }

        if (loginButton != null) loginButton.interactable = false;
        ShowLoginStatus("Logging in...", Color.yellow);

        bool success = await AuthManager.instance.LoginAsync(email, password);

        if (success)
        {
            ShowLoginStatus($"Login sukses! Selamat datang {AuthManager.instance.playerName}", Color.green);
            EC2Client.Instance.SetPlayerName(AuthManager.instance.playerName);
            await OnAuthSuccess();
        }
        else
        {
            ShowLoginStatus("Login gagal. Periksa email/password Anda.", Color.red);
        }

        if (loginButton != null) loginButton.interactable = true;
    }

    // ========== REGISTER ==========
    private async Task OnRegisterClicked()
    {
        if (registerEmailInput == null || registerUsernameInput == null || registerPasswordInput == null)
        {
            ShowRegisterStatus("UI fields tidak terhubung!", Color.red);
            return;
        }

        string email = registerEmailInput.text.Trim();
        string username = registerUsernameInput.text.Trim();
        string password = registerPasswordInput.text.Trim();
        string confirmPassword = registerConfirmPasswordInput != null ? registerConfirmPasswordInput.text.Trim() : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowRegisterStatus("Semua field harus diisi!", Color.red);
            return;
        }

        if (password.Length < 8)
        {
            ShowRegisterStatus("Password minimal 8 karakter!", Color.red);
            return;
        }

        if (!string.IsNullOrEmpty(confirmPassword) && password != confirmPassword)
        {
            ShowRegisterStatus("Password dan konfirmasi password tidak cocok!", Color.red);
            return;
        }

        if (registerButton != null) registerButton.interactable = false;
        ShowRegisterStatus("Registering...", Color.yellow);

        bool success = await AuthManager.instance.RegisterAsync(email, username, password);

        if (success)
        {
            ShowRegisterStatus($"Registrasi sukses! Selamat datang {AuthManager.instance.playerName}", Color.green);
            EC2Client.Instance.SetPlayerName(AuthManager.instance.playerName);
            await OnAuthSuccess();
        }
        else
        {
            ShowRegisterStatus("Registrasi gagal. Email mungkin sudah terdaftar.", Color.red);
        }

        if (registerButton != null) registerButton.interactable = true;
    }

    // ========== AUTH SUCCESS ==========
    private async Task OnAuthSuccess()
    {
        await Task.Delay(1000);

        GameDataManager.instance.InitializeDataHandler();

        if (TutorialManager.instance != null)
            TutorialManager.instance.StartTutorial();

        // Hide auth panel
        if (authPanel != null) authPanel.SetActive(false);

        Debug.Log($"[ModeSelectionUI] Auth success for: {AuthManager.instance.playerName}");
    }

    // ========== UI HELPERS ==========
    private void ShowLoginStatus(string message, Color color)
    {
        if (loginStatusText != null)
        {
            loginStatusText.text = message;
            loginStatusText.color = color;
        }
        Debug.Log($"[ModeSelectionUI-Login] {message}");
    }

    private void ShowRegisterStatus(string message, Color color)
    {
        if (registerStatusText != null)
        {
            registerStatusText.text = message;
            registerStatusText.color = color;
        }
        Debug.Log($"[ModeSelectionUI-Register] {message}");
    }

    // ========== CLEANUP ==========
    private void OnDestroy()
    {
        if (loginButton != null) loginButton.onClick.RemoveAllListeners();
        if (registerButton != null) registerButton.onClick.RemoveAllListeners();
    }
}
