using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject loadingIndicator;

    private bool isRegisterMode = false;

    private void Start()
    {
        loginButton.onClick.AddListener(() => OnLoginClicked());
        registerButton.onClick.AddListener(() => OnRegisterClicked());

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    /// <summary>
    /// Handle login button click
    /// </summary>
    private async void OnLoginClicked()
    {
        if (isRegisterMode)
        {
            // Register mode
            await HandleRegister();
        }
        else
        {
            // Login mode
            await HandleLogin();
        }
    }

    private async Task HandleLogin()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Email dan password harus diisi!", Color.red);
            return;
        }

        ShowLoading(true);
        ShowStatus("Logging in...", Color.yellow);

        bool success = await AuthManager.instance.LoginAsync(email, password);

        if (success)
        {
            ShowStatus($"Login sukses! Selamat datang {AuthManager.instance.playerName}", Color.green);
            EC2Client.Instance.SetPlayerName(AuthManager.instance.playerName);
            // Transition ke game scene
            await Task.Delay(1500);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // Adjust scene name
        }
        else
        {
            ShowStatus("Login gagal. Periksa email/password Anda.", Color.red);
        }

        ShowLoading(false);
    }

    private async Task HandleRegister()
    {
        string email = emailInput.text.Trim();
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Semua field harus diisi!", Color.red);
            return;
        }

        if (password.Length < 8)
        {
            ShowStatus("Password minimal 8 karakter!", Color.red);
            return;
        }

        ShowLoading(true);
        ShowStatus("Registering...", Color.yellow);

        bool success = await AuthManager.instance.RegisterAsync(email, username, password);

        if (success)
        {
            ShowStatus($"Registrasi sukses! Selamat datang {AuthManager.instance.playerName}", Color.green);
            EC2Client.Instance.SetPlayerName(AuthManager.instance.playerName);
            // Transition ke game scene
            await Task.Delay(1500);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // Adjust scene name
        }
        else
        {
            ShowStatus("Registrasi gagal. Coba lagi.", Color.red);
        }

        ShowLoading(false);
    }

    private void OnRegisterClicked()
    {
        isRegisterMode = !isRegisterMode;

        if (isRegisterMode)
        {
            // Tampilkan username field untuk register
            usernameInput.gameObject.SetActive(true);
            loginButton.GetComponentInChildren<Text>().text = "DAFTAR";
            registerButton.GetComponentInChildren<Text>().text = "Ke Login";
            ShowStatus("Mode: REGISTRASI", Color.cyan);
        }
        else
        {
            // Sembunyikan username field untuk login
            usernameInput.gameObject.SetActive(false);
            loginButton.GetComponentInChildren<Text>().text = "LOGIN";
            registerButton.GetComponentInChildren<Text>().text = "Belum punya akun?";
            ShowStatus("Mode: LOGIN", Color.cyan);
        }

        // Clear inputs
        emailInput.text = "";
        usernameInput.text = "";
        passwordInput.text = "";
        statusText.text = "";
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"[LoginUI] {message}");
    }

    private void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);

        loginButton.interactable = !show;
        registerButton.interactable = !show;
    }

    private void OnDestroy()
    {
        if (loginButton != null) loginButton.onClick.RemoveAllListeners();
        if (registerButton != null) registerButton.onClick.RemoveAllListeners();
    }
}
