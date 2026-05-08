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

        // // Reset semua unit jadi otomatis dulu
        // foreach (var unit in allUnits) unit.isManualControlled = false;

        // Pilih yang baru
        selectedUnit = allUnits[index];
        selectedUnit.isManualControlled = true;
        Debug.Log("Sekarang mengendalikan: " + selectedUnit.gameObject.name);
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