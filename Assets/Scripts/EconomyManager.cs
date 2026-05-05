using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager instance;

    public float currentMoney = 500f;
    public TextMeshProUGUI moneyDisplay;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateUI();
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