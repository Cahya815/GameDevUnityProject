using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour 
{
    public static UnitManager instance;

    public List<UnitIdentity> allUnits = new List<UnitIdentity>();
    public UnitIdentity selectedUnit;
    
    [Header("Disaster Unit Unlock Settings")]
    public float disasterUnlockCost = 510f;
    public bool isDisasterUnitUnlocked = false;

    void Awake()
    {
        if (instance == null) instance = this;

        // Pastikan UnitStatsUIManager terpasang secara otomatis biar praktis!
        if (Object.FindFirstObjectByType<UnitStatsUIManager>() == null)
        {
            gameObject.AddComponent<UnitStatsUIManager>();
            Debug.Log("<color=green>UnitStatsUIManager ditambahkan ke UnitManager secara otomatis!</color>");
        }
    }

    void Start()
    {
        // Kunci unit Disaster Control saat awal game
        UpdateDisasterUnitState();
    }

    void Update() {
    // Tombol angka 1-3 buat pilih unit
    if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUnit(0);
    if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUnit(1);
    if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUnit(2);

    // ESCAPE untuk melepas pilihan kursor (unit tetap bekerja otomatis)
    if (Input.GetKeyDown(KeyCode.Escape)) {
        DeselectAll();
    }

    // Tombol 0 untuk memanggil semua unit pulang ke HQ (Cancel tugas)
    if (Input.GetKeyDown(KeyCode.Alpha0)) {
        RecallAllUnits();
    }
    
    // Klik kanan gerakin unit
    if (selectedUnit != null && Input.GetMouseButtonDown(1)) {
        MoveSelectedUnit();
    }
}

public void DeselectAll() {
    // Lepas kontrol manual dari unit yang sedang dipilih agar mereka beralih ke mode otomatis
    if (selectedUnit != null) {
        selectedUnit.isManualControlled = false;
    }
    
    selectedUnit = null; // Kosongkan pilihan
    Debug.Log("<color=cyan>Pilihan dilepas. Unit tetap melanjutkan pekerjaannya!</color>");
}

public void RecallAllUnits() {
    // Paksa semua unit membatalkan tugas mereka dan kembali ke HQ
    foreach (var unit in allUnits) {
        if (unit != null) {
            unit.isManualControlled = false;
            unit.targetObject = null; // Hapus target agar kembali ke HQ
            unit.ReturnToHome();
        }
    }
    
    selectedUnit = null; // Kosongkan pilihan
    Debug.Log("<color=yellow>Semua unit dipanggil pulang ke HQ!</color>");
}

   void SelectUnit(int index) {
    if (index >= allUnits.Count) return;

    // Unit yang akan kita pilih
    UnitIdentity unitToSelect = allUnits[index];

    if (unitToSelect == null) return;

    // Jika unit yang dipilih adalah Disaster Control dan masih terkunci
    if (unitToSelect.jenisUnit == UnitType.DisasterControl && !isDisasterUnitUnlocked)
    {
        Debug.LogWarning($"<color=orange>Disaster Unit masih terkunci! Mencoba membuka dengan biaya ${disasterUnlockCost}...</color>");
        if (TryUnlockDisasterUnit())
        {
            // Jika berhasil dibuka, pilih unit tersebut
            SelectUnit(index);
        }
        return;
    }

    // Lepas status kontrol manual dari unit lain (JANGAN hapus targetObject mereka agar tetap bekerja otomatis!)
    foreach (var unit in allUnits) {
        if (unit != null && unit != unitToSelect) {
            unit.isManualControlled = false; 
        }
    }

    // Aktifkan unit yang dipilih
    selectedUnit = unitToSelect;
    selectedUnit.isManualControlled = true; 
    
    // Pastikan agent unit yang baru dipilih tidak sedang berhenti (Stop)
    if(selectedUnit.agent != null) selectedUnit.agent.isStopped = false;

    Debug.Log("Mengendalikan: " + selectedUnit.gameObject.name);
}

    public bool TryUnlockDisasterUnit()
    {
        if (isDisasterUnitUnlocked) return true;

        if (HQController.currentHQLevel < 2)
        {
            Debug.LogWarning($"<color=red>HQ Level {HQController.currentHQLevel} terlalu rendah untuk membuka Disaster Unit! Dibutuhkan HQ Level 2.</color>");
            return false;
        }

        if (EconomyManager.instance != null && EconomyManager.instance.SpendMoney(disasterUnlockCost))
        {
            isDisasterUnitUnlocked = true;
            UpdateDisasterUnitState();
            Debug.Log($"<color=green>Disaster Unit Berhasil Dibuka seharga ${disasterUnlockCost}!</color>");
            return true;
        }
        else
        {
            Debug.LogWarning($"<color=yellow>Uang tidak cukup untuk membuka Disaster Unit! (Butuh: ${disasterUnlockCost})</color>");
            return false;
        }
    }

    public void UpdateDisasterUnitState()
    {
        foreach (var unit in allUnits)
        {
            if (unit != null && unit.jenisUnit == UnitType.DisasterControl)
            {
                unit.gameObject.SetActive(isDisasterUnitUnlocked);
                if (isDisasterUnitUnlocked && unit.agent != null)
                {
                    unit.agent.isStopped = false;
                }
            }
        }
    }

    void MoveSelectedUnit() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit)) {
        // Beri tahu unit untuk bergerak ke titik klik
        selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);
        
        // Cek apakah yang diklik adalah rumah/pohon
        Flammable f = hit.collider.GetComponent<Flammable>();
        if (f == null) f = hit.collider.GetComponentInParent<Flammable>();
        
        if (f != null && f.isTree && selectedUnit.jenisUnit == UnitType.DisasterControl) {
            Debug.Log("<color=yellow>Unit pembersih tidak bisa membersihkan pohon gosong!</color>");
            selectedUnit.targetObject = null;
        } else {
            selectedUnit.targetObject = f;
        }

        // Setel ke kontrol manual karena pemain baru saja memberi perintah manual baru
        selectedUnit.isManualControlled = true;

        // Sinkronisasi target ke komponen spesifik mobil
        if (selectedUnit.TryGetComponent(out FireTruck ft)) {
            ft.SetTarget(selectedUnit.targetObject);
        }
        if (selectedUnit.TryGetComponent(out DisasterUnit du)) {
            du.SetTarget(selectedUnit.targetObject);
        }
    }
}
}