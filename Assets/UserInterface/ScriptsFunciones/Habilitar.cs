using UnityEngine;
using System.Collections;

public class ControladorSprite : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject objetoAControlar; 
    public bool activarAlTocarse;       

    [Header("Animación de Escala")]
    [Range(0.05f, 1f)] public float duracionAnimacion = 0.2f;

    // VARIABLE PRIVADA: Aquí cada cartel guardará de forma única su escala del Inspector
    private Vector3 escalaOriginalDelCartel = Vector3.one;

    // MÁGIA GLOBAL: Esta variable la comparten TODOS los scripts de la escena
    public static bool hayUnCartelActivo = false;

    private void Awake()
    {
        // Al iniciar el juego, guardamos la escala que tú le configuraste en el Inspector
        if (objetoAControlar != null)
        {
            escalaOriginalDelCartel = objetoAControlar.transform.localScale;
        }
    }

    private void OnMouseDown()
    {
        // 1. Bloqueo si ya hay un cartel activo en pantalla
        if (hayUnCartelActivo && activarAlTocarse)
        {
            Debug.Log("Acción rechazada: No puedes abrir otro cartel hasta cerrar el actual.");
            return;
        }

        // 2. Lanzamos el rayo para evitar el traspaso de clics
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit impacto;

        if (Physics.Raycast(rayo, out impacto))
        {
            if (gameObject != impacto.collider.gameObject && 
                (impacto.collider.gameObject.name.Contains("Sprite") || impacto.collider.CompareTag("SpritePopUp")))
            {
                return; 
            }
        }

        // 3. Si todo está despejado, realizamos la acción con animación suave
        if (objetoAControlar != null)
        {
            StopAllCoroutines(); 

            if (activarAlTocarse)
            {
                hayUnCartelActivo = true;
                Debug.Log("¡Interfaz bloqueada! Cartel abierto: " + objetoAControlar.name);
                
                objetoAControlar.SetActive(true);
                // CAMBIO: En lugar de Vector3.one, escalamos hasta su escala original memorizada
                StartCoroutine(AnimarEscalaSuave(objetoAControlar.transform, Vector3.zero, escalaOriginalDelCartel, true));
            }
            else
            {
                Debug.Log("¡Interfaz liberada! Todo despejado.");
                StartCoroutine(AnimarEscalaSuave(objetoAControlar.transform, objetoAControlar.transform.localScale, Vector3.zero, false));
            }
        }
    }

    private IEnumerator AnimarEscalaSuave(Transform target, Vector3 escalaInicial, Vector3 escalaFinal, bool abriendo)
    {
        target.localScale = escalaInicial;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionAnimacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float porcentaje = tiempoTranscurrido / duracionAnimacion;
            
            float curvaSuave = Mathf.SmoothStep(0f, 1f, porcentaje);
            
            target.localScale = Vector3.Lerp(escalaInicial, escalaFinal, curvaSuave);
            yield return null; 
        }

        target.localScale = escalaFinal;

        if (!abriendo)
        {
            objetoAControlar.SetActive(false);
            hayUnCartelActivo = false; 
        }
    }

    // Método público optimizado para limpiar el estado desde la UI
    public static void ResetearBloqueoGlobal()
    {
        ControladorSprite[] todosLosControles = FindObjectsByType<ControladorSprite>(FindObjectsSortMode.None);
        
        foreach (ControladorSprite control in todosLosControles)
        {
            if (control.objetoAControlar != null && control.objetoAControlar.activeSelf)
            {
                control.StopAllCoroutines();
                
                // Forzamos el reseteo visual inmediato a cero para esconderlo limpiamente
                control.objetoAControlar.transform.localScale = Vector3.zero;
                control.objetoAControlar.SetActive(false);
                Debug.Log("Cartel residual apagado automáticamente: " + control.objetoAControlar.name);
            }
        }

        hayUnCartelActivo = false;
        Debug.Log("¡Estado global reiniciado! Todo despejado y limpio.");
    }
}