using UnityEngine;

public class Hydrant : MonoBehaviour
{
    [Header("Hydrant Settings")]
    public string hydrantName = "Fire Hydrant";
    
    void Start()
    {
        Debug.Log($"Hydrant {hydrantName} inisialisasi pada posisi {transform.position}");
    }
}
