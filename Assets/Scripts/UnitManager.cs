using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour 
{
    public List<UnitIdentity> allUnits = new List<UnitIdentity>();
    public UnitIdentity selectedUnit;

    void Start() {
        Debug.Log("Total unit terdaftar: " + allUnits.Count);
    }

    void Update() {
        // 1. PILIH UNIT (Keyboard)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUnit(0); // Pilih unit pertama
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUnit(1); // Pilih unit kedua
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUnit(2); // Pilih unit ketiga

        // 2. GERAKIN UNIT (Klik Kanan)
        if (selectedUnit != null && Input.GetMouseButtonDown(1)) {
            MoveSelectedUnit();
        }
    }

    void SelectUnit(int index) {
        // Validasi index agar tidak keluar dari batas list
        if (index < 0 || index >= allUnits.Count) {
            Debug.LogWarning("Index unit tidak valid: " + index);
            return;
        }

        // Reset semua unit ke idle
        foreach (var unit in allUnits) {
            unit.isManualControlled = false;
            unit.isIdle = true; // Set semua unit ke idle
        }

        // Ganti ke unit baru
        selectedUnit = allUnits[index];
        selectedUnit.isManualControlled = true;
        selectedUnit.isIdle = false; // Hanya unit yang dipilih yang tidak idle

        Debug.Log("Unit Terpilih: " + selectedUnit.name);
    }

    void MoveSelectedUnit() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            // Perintahkan unit untuk bergerak ke lokasi yang diklik
            selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);

            // Ambil komponen Flammable dari apa yang diklik
            Flammable f = hit.collider.GetComponentInParent<Flammable>();

            // HANYA unit yang dipilih yang mendapatkan target ini
            selectedUnit.targetObject = f;

            if (f != null) Debug.Log(selectedUnit.name + " fokus ke rumah " + hit.collider.name);
        }
    }
}