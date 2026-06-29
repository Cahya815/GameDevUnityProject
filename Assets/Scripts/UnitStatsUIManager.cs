using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitStatsUIManager : MonoBehaviour
{
    public static UnitStatsUIManager instance;

    [Header("UI Panel References")]
    public GameObject panelObject;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI crewText;
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI engineText;

    public Image waterBarFill;
    public Image engineBarFill;

    public Button repairButton;
    public TextMeshProUGUI repairButtonText;
    public Button trainButton;
    public TextMeshProUGUI trainButtonText;

    public GameObject waterBarParent;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure the panel is initially hidden. You will assign the panelObject in the Inspector.
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    void Update()
    {
        if (panelObject == null) return;

        // Ambil unit yang dipilih dari singleton UnitManager
        if (UnitManager.instance == null || UnitManager.instance.selectedUnit == null)
        {
            panelObject.SetActive(false);
            return;
        }

        panelObject.SetActive(true);
        UnitIdentity unit = UnitManager.instance.selectedUnit;

        // 1. Update Title (Nama Unit)
        titleText.text = unit.gameObject.name.Replace("(Clone)", "");

        // 2. Update status Latihan / Crew Level
        if (unit.isTraining)
        {
            crewText.text = $"<color=yellow>Latihan... {unit.trainingTimer:F1}s / {unit.trainingDuration:F0}s</color>";
            trainButton.interactable = false;
            trainButtonText.text = "Sedang Latihan...";
        }
        else
        {
            crewText.text = $"Anggota: <color=green>Level {unit.crewLevel}</color> (Power: {unit.power:F0})";
            float trainCost = unit.crewLevel * 100f;
            trainButtonText.text = $"Latih Crew\n<size=10f>Biaya: ${trainCost}</size>";

            // Nonaktifkan jika uang kurang atau sedang mogok
            bool canAffordTrain = EconomyManager.instance != null && EconomyManager.instance.currentMoney >= trainCost;
            trainButton.interactable = canAffordTrain && !unit.isStalled;
        }

        // 3. Update Engine Condition & Bar
        if (unit.isStalled)
        {
            // Teks mogok kedap-kedip pakai sin fasa waktu
            float blink = Mathf.Abs(Mathf.Sin(Time.time * 5f));
            string colorCode = blink > 0.5f ? "red" : "yellow";
            engineText.text = $"Kondisi Mesin: <color={colorCode}>MOGOK (Butuh Service!)</color>";
            engineBarFill.color = Color.red;
        }
        else
        {
            engineText.text = $"Kondisi Mesin: {unit.engineCondition:F0}%";
            // Ubah warna bar dinamis (Merah jika kritis, hijau jika bagus)
            if (unit.engineCondition > 50f)
                engineBarFill.color = Color.green;
            else if (unit.engineCondition > 20f)
                engineBarFill.color = new Color(1f, 0.6f, 0f); // Orange
            else
                engineBarFill.color = Color.red;
        }
        engineBarFill.fillAmount = unit.engineCondition / 100f;

        // Biaya rehabilitasi / service mesin
        float repairCost = (100f - unit.engineCondition) * 1f;
        if (repairCost < 5f) repairCost = 5f;
        repairButtonText.text = unit.isStalled
            ? $"Rehabilitasi Mesin\n<size=10f>Biaya: ${repairCost:F0}</size>"
            : $"Rawat Mesin\n<size=10f>Biaya: ${repairCost:F0}</size>";

        bool canAffordRepair = EconomyManager.instance != null && EconomyManager.instance.currentMoney >= repairCost;
        repairButton.interactable = canAffordRepair && unit.engineCondition < 98f;

        // 4. Update Water Tank (Hanya tampil untuk Firefighter / Mobil Pemadam)
        if (unit.jenisUnit == UnitType.Firefighter)
        {
            waterBarParent.SetActive(true);
            FireTruck ft = unit.GetComponent<FireTruck>();
            if (ft != null)
            {
                waterText.text = $"Tangki Air: {ft.currentWater:F0} / {ft.maxWater:F0} Liter";
                waterBarFill.fillAmount = ft.currentWater / ft.maxWater;

                // Ubah tinggi panel jika ada tanki air biar gak sesak
                RectTransform panelRect = panelObject.GetComponent<RectTransform>();
                if (panelRect.sizeDelta.y != 160f) panelRect.sizeDelta = new Vector2(500f, 160f);
            }
        }
        else
        {
            waterBarParent.SetActive(false);
            // Perkecil tinggi panel jika tidak ada tangki air (Disaster unit)
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            if (panelRect.sizeDelta.y != 120f) panelRect.sizeDelta = new Vector2(500f, 120f);
        }
    }

    private void OnRepairClicked()
    {
        if (UnitManager.instance != null && UnitManager.instance.selectedUnit != null)
        {
            UnitManager.instance.selectedUnit.RehabilitateEngine();
        }
    }

    private void OnTrainClicked()
    {
        if (UnitManager.instance != null && UnitManager.instance.selectedUnit != null)
        {
            UnitManager.instance.selectedUnit.StartTraining();
        }
    }
}