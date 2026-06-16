using UnityEngine;

public class ControladorSprite : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject objetoAControlar; 
    public bool activarAlTocarse;       

    // Volvemos a la detección directa y rápida
    private void OnMouseDown()
    {
        if (objetoAControlar != null)
        {
            objetoAControlar.SetActive(activarAlTocarse);
        }
    }
}