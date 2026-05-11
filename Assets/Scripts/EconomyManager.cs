using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager instance;

    //gatau eror engga148
    private int awokawok = 0;
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
        _dataHandler = new LocalSaveProvider();
    }

    public async void OnMissionComplete(int apiPadam, float waktu)
    {
        // Panggil fungsi simpan tanpa peduli simpan ke mana
        await _dataHandler.SaveMissionResult("Pemain1", apiPadam, waktu);
    }
}

    // FUNGSI INI YANG HILANG DAN MENYEBABKAN ERROR
    public bool SpendMoney(float amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("<color=red>Uang tidak cukup!</color>");
            return false;
        }
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
}