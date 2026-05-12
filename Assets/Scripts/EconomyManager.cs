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
        // Setup provider (Sementara local)
        _dataHandler = new LocalSaveProvider();
    }

    public async void OnMissionComplete(int apiPadam, float waktu)
    {
        // Hitung bonus uang berdasarkan api yang padam
        float bonus = apiPadam * 10f; 
        AddMoney(bonus);

        // Simpan data ke "Backend" (Local/Cloud)
        await _dataHandler.SaveMissionResult("Pemain1", apiPadam, waktu);
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