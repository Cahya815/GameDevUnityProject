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
        // Cek apakah yang diklik adalah rumah/pohon
        Flammable f = hit.collider.GetComponent<Flammable>();
        if (f == null) f = hit.collider.GetComponentInParent<Flammable>();

        // Beri tahu unit untuk bergerak ke titik klik atau posisi Flammable
        var agent = selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) {
            if (f != null) {
                // Hitung parkir dinamis sesuai arah kedatangan unit saat ini agar tidak selalu parkir di belakang
                Vector3 targetPos = f.transform.position;
                Vector3 currentPos = agent.transform.position;
                Vector3 direction = (currentPos - targetPos).normalized;
                direction.y = 0;
                if (direction == Vector3.zero) direction = agent.transform.forward;

                float offsetDist = 3.5f; // Jarak aman di luar collider rumah
                Vector3 targetDestination = targetPos + direction.normalized * offsetDist;

                if (UnityEngine.AI.NavMesh.SamplePosition(targetDestination, out UnityEngine.AI.NavMeshHit navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                }
                else
                {
                    agent.SetDestination(targetPos);
                }
            } else {
                agent.SetDestination(hit.point);
            }
        }
        
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
            if (selectedUnit.targetObject != null)
                ft.SetTarget(selectedUnit.targetObject);
            else
                ft.SetTarget(hit.point);
        }
        if (selectedUnit.TryGetComponent(out DisasterUnit du)) {
            if (selectedUnit.targetObject != null)
                du.SetTarget(selectedUnit.targetObject);
            else
                du.SetTarget(hit.point);
        }

        if (f != null) {
            string canBeExtinguished = "";
            if (selectedUnit.jenisUnit == UnitType.Firefighter) {
                if (f.currentStatus == HouseStatus.Terbakar) {
                    canBeExtinguished = " (Bisa Dipadamkan)";
                } else if (f.currentStatus == HouseStatus.AdaUlar) {
                    canBeExtinguished = " (Ada Ular - Bisa Diamankan)";
                } else if (f.currentStatus == HouseStatus.Puing) {
                    canBeExtinguished = " (Sudah Hangus Jadi Puing - Tidak Bisa Dipadamkan)";
                } else {
                    canBeExtinguished = " (Kondisi Aman - Tidak Perlu Dipadamkan)";
                }
            } else if (selectedUnit.jenisUnit == UnitType.DisasterControl) {
                if (f.currentStatus == HouseStatus.Puing) {
                    if (f.isTree) {
                        canBeExtinguished = " (Pohon Gosong - Tidak Perlu Dibersihkan)";
                    } else {
                        canBeExtinguished = " (Bisa Dibersihkan)";
                    }
                } else {
                    canBeExtinguished = " (Bukan Puing - Tidak Perlu Dibersihkan)";
                }
            }
            Debug.Log("Unit " + selectedUnit.name + " diarahkan ke: " + f.gameObject.name + " di " + f.transform.position + canBeExtinguished);
        } else {
            Debug.Log("Unit " + selectedUnit.name + " diperintahkan ke: " + hit.point);
        }
    }
}
}