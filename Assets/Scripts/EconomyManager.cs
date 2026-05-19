using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager instance;

    private IGameDataHandler _dataHandler;

    public float currentMoney = 500f;
    public TextMeshProUGUI moneyDisplay;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
        if (AuthManager.instance != null)
        {
            if (AuthManager.instance.isOnlineMode)
                _dataHandler = new SupabaseProvider();
            else
                _dataHandler = new LocalSaveProvider();
        }
        else
        {
            _dataHandler = new LocalSaveProvider(); // Default to local
        }
    }

    public async void OnMissionComplete(int apiPadam, float waktu)
    {
        // Hitung bonus uang berdasarkan api yang padam
        float bonus = apiPadam * 10f; 
        AddMoney(bonus);

        // Simpan data ke "Backend" (Local/Cloud)
        string pName = AuthManager.instance != null ? AuthManager.instance.playerName : "Pemain1";
        if (_dataHandler != null)
        {
            await _dataHandler.SaveMissionResult(pName, apiPadam, waktu);
        }
        else if (GameDataManager.instance != null)
        {
            GameDataManager.instance.SaveGameResult(apiPadam, waktu);
        }
        
        Debug.Log($"Misi Selesai! Dapat bonus: ${bonus}");
    }

    public bool SpendMoney(float amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        
        Debug.Log("<color=red>Uang tidak cukup!</color>");
        return false;
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (moneyDisplay != null)
            moneyDisplay.text = "Money: $" + currentMoney.ToString("F0");
    }
} // <--- Pastikan hanya ada satu kurung tutup di paling akhir class