using UnityEngine;

public class ControladorSprite : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject objetoAControlar;
    public bool deshabilitarAlTocarse = true; // si está en true, desactiva; si está en false, activa

    private void OnMouseDown()
    {
        if (objetoAControlar != null)
        {
            objetoAControlar.SetActive(!deshabilitarAlTocarse);
        }
    }
}