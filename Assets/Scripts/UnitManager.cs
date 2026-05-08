using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour 
{
    public List<UnitIdentity> allUnits = new List<UnitIdentity>();
    public UnitIdentity selectedUnit;
    
    void Update() {
        // Pilih unit pakai tombol angka
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectUnit(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectUnit(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectUnit(2);

        // Klik kanan untuk gerakkan unit yang dipilih secara manual
        if (selectedUnit != null && Input.GetMouseButtonDown(1)) {
            MoveSelectedUnit();
        }
    }

   void SelectUnit(int index) {
    if (index >= allUnits.Count) return;

    // Unit yang akan kita pilih
    UnitIdentity unitToSelect = allUnits[index];

    // Reset SEMUA unit lain agar pulang
    foreach (var unit in allUnits) {
        if (unit != null && unit != unitToSelect) {
            unit.isManualControlled = false; 
            unit.targetObject = null; // Unit lain harus lepas target agar pulang
        }
    }

    // Aktifkan unit yang dipilih
    selectedUnit = unitToSelect;
    selectedUnit.isManualControlled = true; 
    
    // Pastikan agent unit yang baru dipilih tidak sedang berhenti (Stop)
    if(selectedUnit.agent != null) selectedUnit.agent.isStopped = false;

    Debug.Log("Mengendalikan: " + selectedUnit.gameObject.name);
}

    void MoveSelectedUnit() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit)) {
        // Beri tahu unit untuk bergerak ke titik klik
        selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);
        
        // Cek apakah yang diklik adalah rumah
        Flammable f = hit.collider.GetComponent<Flammable>();
        selectedUnit.targetObject = f; // Jika klik tanah, ini jadi null (bagus!)
    }
}
}