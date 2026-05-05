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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUnit(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUnit(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUnit(2);

        // 2. GERAKIN UNIT (Klik Kanan)
        // PENTING: Gunakan selectedUnit, JANGAN pakai activeUnit dari GridManager
        if (selectedUnit != null && Input.GetMouseButtonDown(1)) {
            MoveSelectedUnit();
        }
    }

    void SelectUnit(int index) {
        if (index >= allUnits.Count) return;

        // Reset status lama
        if (selectedUnit != null) selectedUnit.isManualControlled = false;

        // Ganti ke unit baru
        selectedUnit = allUnits[index];
        selectedUnit.isManualControlled = true;

        Debug.Log("Unit Terpilih: " + selectedUnit.name);
    }

    void MoveSelectedUnit() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);

            // Ambil komponen Flammable dari apa yang diklik
            Flammable f = hit.collider.GetComponentInParent<Flammable>();

            // HANYA unit yang dipilih yang dapet target ini!
            selectedUnit.targetObject = f;

            if (f != null) Debug.Log(selectedUnit.name + " fokus ke rumah " + hit.collider.name);
        }
    }
}