using UnityEngine;

public class DragScale : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadRotacion = 0.5f;

    [Header("Configuración de Escala")]
    public float escalaMinima = 0.3f;
    public float escalaMaxima = 3f;
    public float velocidadEscalaMouse = 0.5f; 
    public float velocidadEscalaTouch = 0.01f;

    private Vector3 posicionMousePrevia;

    void Update()
    {
        if (ControladorSprite.hayUnCartelActivo) return;

        // --- MÓVILES (TOUCH) ---
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                Touch toque = Input.GetTouch(0);
                if (toque.phase == TouchPhase.Moved)
                {
                    float rotacionX = toque.deltaPosition.x * velocidadRotacion;
                    float rotacionY = toque.deltaPosition.y * velocidadRotacion;

                    // CORRECCIÓN DE ROTACIÓN: Signos ajustados para naturalidad táctil
                    transform.Rotate(Vector3.up, rotacionX, Space.World); 
                    transform.Rotate(Vector3.right, -rotacionY, Space.World);
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch toque0 = Input.GetTouch(0);
                Touch toque1 = Input.GetTouch(1);

                Vector2 toque0PosPrevia = toque0.position - toque0.deltaPosition;
                Vector2 toque1PosPrevia = toque1.position - toque1.deltaPosition;

                float magnitudPrevia = (toque0PosPrevia - toque1PosPrevia).magnitude;
                float magnitudActual = (toque0.position - toque1.position).magnitude;

                // CORRECCIÓN CLAVE DE ESCALA TÁCTIL (Pinch-to-Zoom Invertido)
                // Antes: magnitudActual - magnitudPrevia
                // Ahora: magnitudPrevia - magnitudActual
                float diferenciaMagnitud = magnitudPrevia - magnitudActual;

                // Usamos la multiplicación relativa para una suavidad profesional en móviles
                EscalarObjetoTouch(diferenciaMagnitud * velocidadEscalaTouch);
            }
        }
        // --- COMPUTADORA (MOUSE) ---
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                posicionMousePrevia = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 deltaMouse = Input.mousePosition - posicionMousePrevia;
                
                float rotacionX = deltaMouse.x * velocidadRotacion;
                float rotacionY = deltaMouse.y * velocidadRotacion;

                // Rotación clásica para el arrastre de mouse
                transform.Rotate(Vector3.up, -rotacionX, Space.World);
                transform.Rotate(Vector3.right, rotacionY, Space.World);

                posicionMousePrevia = Input.mousePosition;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                // Mantenemos el escalado por suma para el scroll físico
                EscalarObjetoMouse(scroll * velocidadEscalaMouse);
            }
        }
    }

    // Método optimizado para la suavidad del Touch en móviles (Multiplicación relativa)
    private void EscalarObjetoTouch(float factorCambio)
    {
        // El factor de cambio ya está invertido por la corrección matemática de arriba
        Vector3 nuevaEscala = transform.localScale * (1f + factorCambio);

        nuevaEscala.x = Mathf.Clamp(nuevaEscala.x, escalaMinima, escalaMaxima);
        nuevaEscala.y = Mathf.Clamp(nuevaEscala.y, escalaMinima, escalaMaxima);
        nuevaEscala.z = Mathf.Clamp(nuevaEscala.z, escalaMinima, escalaMaxima);

        transform.localScale = nuevaEscala;
    }

    // Método clásico por suma para el scroll físico del mouse
    private void EscalarObjetoMouse(float factorCambio)
    {
        Vector3 nuevaEscala = transform.localScale + Vector3.one * factorCambio;

        nuevaEscala.x = Mathf.Clamp(nuevaEscala.x, escalaMinima, escalaMaxima);
        nuevaEscala.y = Mathf.Clamp(nuevaEscala.y, escalaMinima, escalaMaxima);
        nuevaEscala.z = Mathf.Clamp(nuevaEscala.z, escalaMinima, escalaMaxima);

        transform.localScale = nuevaEscala;
    }
}