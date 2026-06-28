using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitStatsUIManager : MonoBehaviour
{
    public static UnitStatsUIManager instance;

    [Header("UI Panel Reference")]
    private GameObject panelObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI crewText;
    private TextMeshProUGUI waterText;
    private TextMeshProUGUI engineText;
    
    private Image waterBarFill;
    private Image engineBarFill;
    
    private Button repairButton;
    private TextMeshProUGUI repairButtonText;
    private Button trainButton;
    private TextMeshProUGUI trainButtonText;

    private GameObject waterBarParent;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Tunggu sebentar agar Canvas utama terbuat oleh game jika ada, atau buat programmatically jika tidak ketemu
        // StartCoroutine(InitializeUI());
    }

    // private System.Collections.IEnumerator InitializeUI()
    // {
    //     yield return new WaitForSeconds(0.1f);
    //
    //     // 1. Cari Canvas utama di scene
    //     Canvas canvas = Object.FindFirstObjectByType<Canvas>();
    //     if (canvas == null)
    //     {
    //         // Jika tidak ada Canvas sama sekali, buat baru
    //         GameObject canvasObj = new GameObject("UnitStatsCanvas");
    //         canvas = canvasObj.AddComponent<Canvas>();
    //         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //         canvasObj.AddComponent<CanvasScaler>();
    //         canvasObj.AddComponent<GraphicRaycaster>();
    //     }
    //
    //     // 2. Buat Panel Utama (Glassmorphism look)
    //     panelObject = new GameObject("SelectedUnitPanel");
    //     panelObject.transform.SetParent(canvas.transform, false);
    //
    //     RectTransform panelRect = panelObject.AddComponent<RectTransform>();
    //     // Posisi bottom-center
    //     panelRect.anchorMin = new Vector2(0.5f, 0f);
    //     panelRect.anchorMax = new Vector2(0.5f, 0f);
    //     panelRect.pivot = new Vector2(0.5f, 0f);
    //     panelRect.anchoredPosition = new Vector2(0f, 20f);
    //     panelRect.sizeDelta = new Vector2(500f, 160f);
    //
    //     // Tambah background image
    //     Image panelBg = panelObject.AddComponent<Image>();
    //     panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f); // Sleek dark mode background
    //
    //     // Tambah outline tipis putih/transparan biar terkesan premium (Glassmorphic)
    //     Outline outline = panelObject.AddComponent<Outline>();
    //     outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
    //     outline.effectDistance = new Vector2(1f, 1f);
    //
    //     // 3. Buat Title Text (Nama Unit)
    //     GameObject titleObj = new GameObject("TitleText");
    //     titleObj.transform.SetParent(panelObject.transform, false);
    //     titleText = titleObj.AddComponent<TextMeshProUGUI>();
    //     titleText.fontSize = 18f;
    //     titleText.fontStyle = FontStyles.Bold;
    //     titleText.color = Color.white;
    //     titleText.alignment = TextAlignmentOptions.Left;
    //
    //     RectTransform titleRect = titleObj.GetComponent<RectTransform>();
    //     titleRect.anchorMin = new Vector2(0f, 1f);
    //     titleRect.anchorMax = new Vector2(0f, 1f);
    //     titleRect.pivot = new Vector2(0f, 1f);
    //     titleRect.anchoredPosition = new Vector2(15f, -10f);
    //     titleRect.sizeDelta = new Vector2(250f, 25f);
    //
    //     // 4. Buat Crew Text (Level Anggota)
    //     GameObject crewObj = new GameObject("CrewText");
    //     crewObj.transform.SetParent(panelObject.transform, false);
    //     crewText = crewObj.AddComponent<TextMeshProUGUI>();
    //     crewText.fontSize = 14f;
    //     crewText.color = new Color(0.8f, 0.8f, 0.9f);
    //     crewText.alignment = TextAlignmentOptions.Left;
    //
    //     RectTransform crewRect = crewObj.GetComponent<RectTransform>();
    //     crewRect.anchorMin = new Vector2(0f, 1f);
    //     crewRect.anchorMax = new Vector2(0f, 1f);
    //     crewRect.pivot = new Vector2(0f, 1f);
    //     crewRect.anchoredPosition = new Vector2(15f, -35f);
    //     crewRect.sizeDelta = new Vector2(250f, 20f);
    //
    //     // 5. Buat Bar Engine Condition
    //     GameObject engineLabelObj = new GameObject("EngineLabel");
    //     engineLabelObj.transform.SetParent(panelObject.transform, false);
    //     engineText = engineLabelObj.AddComponent<TextMeshProUGUI>();
    //     engineText.fontSize = 13f;
    //     engineText.color = Color.white;
    //
    //     RectTransform engineLabelRect = engineLabelObj.GetComponent<RectTransform>();
    //     engineLabelRect.anchorMin = new Vector2(0f, 1f);
    //     engineLabelRect.anchorMax = new Vector2(0f, 1f);
    //     engineLabelRect.pivot = new Vector2(0f, 1f);
    //     engineLabelRect.anchoredPosition = new Vector2(15f, -60f);
    //     engineLabelRect.sizeDelta = new Vector2(250f, 20f);
    //
    //     // Bar Background Engine
    //     GameObject engineBarBgObj = new GameObject("EngineBarBg");
    //     engineBarBgObj.transform.SetParent(panelObject.transform, false);
    //     Image engineBarBg = engineBarBgObj.AddComponent<Image>();
    //     engineBarBg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
    //
    //     RectTransform engineBarBgRect = engineBarBgObj.GetComponent<RectTransform>();
    //     engineBarBgRect.anchorMin = new Vector2(0f, 1f);
    //     engineBarBgRect.anchorMax = new Vector2(0f, 1f);
    //     engineBarBgRect.pivot = new Vector2(0f, 1f);
    //     engineBarBgRect.anchoredPosition = new Vector2(15f, -80f);
    //     engineBarBgRect.sizeDelta = new Vector2(250f, 12f);
    //
    //     // Bar Fill Engine
    //     GameObject engineBarFillObj = new GameObject("EngineBarFill");
    //     engineBarFillObj.transform.SetParent(engineBarBgObj.transform, false);
    //     engineBarFill = engineBarFillObj.AddComponent<Image>();
    //     engineBarFill.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green fill
    //
    //     RectTransform engineBarFillRect = engineBarFillObj.GetComponent<RectTransform>();
    //     engineBarFillRect.anchorMin = new Vector2(0f, 0f);
    //     engineBarFillRect.anchorMax = new Vector2(1f, 1f); // Stretch
    //     engineBarFillRect.pivot = new Vector2(0f, 0.5f);
    //     engineBarFillRect.offsetMin = Vector2.zero;
    //     engineBarFillRect.offsetMax = Vector2.zero;
    //
    //     // 6. Buat Bar Water Tank (Hanya untuk damkar, buat container-nya dulu)
    //     waterBarParent = new GameObject("WaterBarParent");
    //     waterBarParent.transform.SetParent(panelObject.transform, false);
    //     RectTransform waterParentRect = waterBarParent.AddComponent<RectTransform>();
    //     waterParentRect.anchorMin = new Vector2(0f, 0f);
    //     waterParentRect.anchorMax = new Vector2(1f, 1f);
    //     waterParentRect.offsetMin = Vector2.zero;
    //     waterParentRect.offsetMax = Vector2.zero;
    //
    //     GameObject waterLabelObj = new GameObject("WaterLabel");
    //     waterLabelObj.transform.SetParent(waterBarParent.transform, false);
    //     waterText = waterLabelObj.AddComponent<TextMeshProUGUI>();
    //     waterText.fontSize = 13f;
    //     waterText.color = Color.cyan;
    //
    //     RectTransform waterLabelRect = waterLabelObj.GetComponent<RectTransform>();
    //     waterLabelRect.anchorMin = new Vector2(0f, 1f);
    //     waterLabelRect.anchorMax = new Vector2(0f, 1f);
    //     waterLabelRect.pivot = new Vector2(0f, 1f);
    //     waterLabelRect.anchoredPosition = new Vector2(15f, -100f);
    //     waterLabelRect.sizeDelta = new Vector2(250f, 20f);
    //
    //     // Bar Background Water
    //     GameObject waterBarBgObj = new GameObject("WaterBarBg");
    //     waterBarBgObj.transform.SetParent(waterBarParent.transform, false);
    //     Image waterBarBg = waterBarBgObj.AddComponent<Image>();
    //     waterBarBg.color = new Color(0.1f, 0.2f, 0.3f, 0.8f);
    //
    //     RectTransform waterBarBgRect = waterBarBgObj.GetComponent<RectTransform>();
    //     waterBarBgRect.anchorMin = new Vector2(0f, 1f);
    //     waterBarBgRect.anchorMax = new Vector2(0f, 1f);
    //     waterBarBgRect.pivot = new Vector2(0f, 1f);
    //     waterBarBgRect.anchoredPosition = new Vector2(15f, -120f);
    //     waterBarBgRect.sizeDelta = new Vector2(250f, 12f);
    //
    //     // Bar Fill Water
    //     GameObject waterBarFillObj = new GameObject("WaterBarFill");
    //     waterBarFillObj.transform.SetParent(waterBarBgObj.transform, false);
    //     waterBarFill = waterBarFillObj.AddComponent<Image>();
    //     waterBarFill.color = new Color(0.1f, 0.6f, 1f, 0.9f); // Blue fill
    //
    //     RectTransform waterBarFillRect = waterBarFillObj.GetComponent<RectTransform>();
    //     waterBarFillRect.anchorMin = new Vector2(0f, 0f);
    //     waterBarFillRect.anchorMax = new Vector2(1f, 1f); // Stretch
    //     waterBarFillRect.pivot = new Vector2(0f, 0.5f);
    //     waterBarFillRect.offsetMin = Vector2.zero;
    //     waterBarFillRect.offsetMax = Vector2.zero;
    //
    //     // 7. Buat Tombol "Rehabilitasi Mesin"
    //     GameObject repairBtnObj = new GameObject("RepairButton");
    //     repairBtnObj.transform.SetParent(panelObject.transform, false);
    //     repairButton = repairBtnObj.AddComponent<Button>();
    //     Image repairBtnImg = repairBtnObj.AddComponent<Image>();
    //     repairBtnImg.color = new Color(0.7f, 0.2f, 0.2f); // Premium Orange-Red button
    //
    //     RectTransform repairBtnRect = repairBtnObj.GetComponent<RectTransform>();
    //     repairBtnRect.anchorMin = new Vector2(1f, 1f);
    //     repairBtnRect.anchorMax = new Vector2(1f, 1f);
    //     repairBtnRect.pivot = new Vector2(1f, 1f);
    //     repairBtnRect.anchoredPosition = new Vector2(-15f, -20f);
    //     repairBtnRect.sizeDelta = new Vector2(180f, 45f);
    //
    //     // Repair Text
    //     GameObject repairTxtObj = new GameObject("RepairText");
    //     repairTxtObj.transform.SetParent(repairBtnObj.transform, false);
    //     repairButtonText = repairTxtObj.AddComponent<TextMeshProUGUI>();
    //     repairButtonText.fontSize = 13f;
    //     repairButtonText.fontStyle = FontStyles.Bold;
    //     repairButtonText.alignment = TextAlignmentOptions.Center;
    //     repairButtonText.color = Color.white;
    //
    //     RectTransform repairTxtRect = repairTxtObj.GetComponent<RectTransform>();
    //     repairTxtRect.anchorMin = Vector2.zero;
    //     repairTxtRect.anchorMax = Vector2.one;
    //     repairTxtRect.offsetMin = Vector2.zero;
    //     repairTxtRect.offsetMax = Vector2.zero;
    //
    //     repairButton.onClick.AddListener(OnRepairClicked);
    //
    //     // 8. Buat Tombol "Latih Anggota"
    //     GameObject trainBtnObj = new GameObject("TrainButton");
    //     trainBtnObj.transform.SetParent(panelObject.transform, false);
    //     trainButton = trainBtnObj.AddComponent<Button>();
    //     Image trainBtnImg = trainBtnObj.AddComponent<Image>();
    //     trainBtnImg.color = new Color(0.15f, 0.55f, 0.35f); // Deep green button
    //
    //     RectTransform trainBtnRect = trainBtnObj.GetComponent<RectTransform>();
    //     trainBtnRect.anchorMin = new Vector2(1f, 1f);
    //     trainBtnRect.anchorMax = new Vector2(1f, 1f);
    //     trainBtnRect.pivot = new Vector2(1f, 1f);
    //     trainBtnRect.anchoredPosition = new Vector2(-15f, -80f);
    //     trainBtnRect.sizeDelta = new Vector2(180f, 45f);
    //
    //     // Train Text
    //     GameObject trainTxtObj = new GameObject("TrainText");
    //     trainTxtObj.transform.SetParent(trainBtnObj.transform, false);
    //     trainButtonText = trainTxtObj.AddComponent<TextMeshProUGUI>();
    //     trainButtonText.fontSize = 13f;
    //     trainButtonText.fontStyle = FontStyles.Bold;
    //     trainButtonText.alignment = TextAlignmentOptions.Center;
    //     trainButtonText.color = Color.white;
    //
    //     RectTransform trainTxtRect = trainTxtObj.GetComponent<RectTransform>();
    //     trainTxtRect.anchorMin = Vector2.zero;
    //     trainTxtRect.anchorMax = Vector2.one;
    //     trainTxtRect.offsetMin = Vector2.zero;
    //     trainTxtRect.offsetMax = Vector2.zero;
    //
    //     trainButton.onClick.AddListener(OnTrainClicked);
    //
    //     panelObject.SetActive(false); // Sembunyikan dulu pas awal game
    //     Debug.Log("<color=green>Sistem UI Panel UnitStats berhasil dibuat secara programmatis!</color>");
    // }

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
