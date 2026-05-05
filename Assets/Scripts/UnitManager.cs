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
    Debug.Log("Mencoba memilih index: " + index + " dari total unit: " + allUnits.Count);
    
    if (index >= allUnits.Count) {
        Debug.LogWarning("Index unit tidak ditemukan! Cek apakah unit sudah masuk List.");
        return;
    }

    foreach (var unit in allUnits) unit.isManualControlled = false;

    selectedUnit = allUnits[index];
    selectedUnit.isManualControlled = true;
}

    
    void MoveSelectedUnit() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit)) {
        selectedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);
        
        // Ambil komponen Flammable dari apa yang diklik
        Flammable f = hit.collider.GetComponentInParent<Flammable>();
        
        // HANYA unit yang dipilih yang dapet target ini!
        selectedUnit.targetObject = f; 
        
        if(f != null) Debug.Log(selectedUnit.name + " fokus ke rumah " + hit.collider.name);
    }
}


}