using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControladorSpritesBotones : MonoBehaviour
{
    [System.Serializable]
    public class EstructuraBoton
    {
        public Button boton;          // El botón Legacy
        public Sprite spriteActivo;   // Su imagen cuando está seleccionado
        public Sprite spriteInactivo; // Su imagen normal
    }

    [Header("Configuración de Botones")]
    public List<EstructuraBoton> listaBotones = new List<EstructuraBoton>();

    void Start()
    {
        // Asignamos el comportamiento a cada botón de la lista
        for (int i = 0; i < listaBotones.Count; i++)
        {
            int index = i;
            listaBotones[i].boton.onClick.AddListener(() => IntercambiarSprites(index));
        }

        // Como el primer botón ya está activo por defecto en tu app,
        // forzamos a que visualmente muestre su sprite activo al arrancar.
        if (listaBotones.Count > 0)
        {
            IntercambiarSprites(0);
        }
    }

    public void IntercambiarSprites(int indiceSeleccionado)
    {
        for (int i = 0; i < listaBotones.Count; i++)
        {
            // Buscamos el componente Image del Legacy Button
            Image imgComponente = listaBotones[i].boton.GetComponent<Image>();
            
            if (imgComponente != null)
            {
                if (i == indiceSeleccionado)
                {
                    // Cambia al sprite de estado activo
                    imgComponente.sprite = listaBotones[i].spriteActivo;
                }
                else
                {
                    // Cambia al sprite de estado inactivo
                    imgComponente.sprite = listaBotones[i].spriteInactivo;
                }
            }
        }
    }
}