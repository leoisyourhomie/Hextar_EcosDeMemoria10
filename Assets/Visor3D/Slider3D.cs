using UnityEngine;

public class NavegadorDioramas : MonoBehaviour
{
    [Header("Lista de Dioramas")]
    public GameObject[] dioramas; 

    private Vector3[] escalasIniciales;
    private Quaternion[] rotacionesIniciales;
    private int indiceActual = 0;

    void Awake()
    {
        if (dioramas.Length == 0)
        {
            Debug.LogError("¡ALERTA! No has arrastrado ningún diorama al script NavegadorDioramas en el Inspector.");
            return;
        }

        escalasIniciales = new Vector3[dioramas.Length];
        rotacionesIniciales = new Quaternion[dioramas.Length];

        for (int i = 0; i < dioramas.Length; i++)
        {
            if (dioramas[i] != null)
            {
                escalasIniciales[i] = dioramas[i].transform.localScale;
                rotacionesIniciales[i] = dioramas[i].transform.rotation;
            }
        }
    }

    void Start()
    {
        InicializarVisor();
    }

    private void InicializarVisor()
    {
        Debug.Log("=== Inicializando Visor 3D ===");
        for (int i = 0; i < dioramas.Length; i++)
        {
            if (dioramas[i] != null)
            {
                dioramas[i].SetActive(i == 0);
                Debug.Log($"Diorama [{i}] ({dioramas[i].name}) estado inicial: {(i == 0 ? "VISIBLE" : "OCULTO")}");
            }
        }
        indiceActual = 0;
        ControladorSprite.ResetearBloqueoGlobal();
    }

    void Update()
{
    // PRUEBA DE EMERGENCIA: Si presionas la barra espaciadora en el teclado de tu compu
    if (Input.GetKeyDown(KeyCode.Space))
    {
        Debug.Log("¡Barra espaciadora detectada! Cambiando de diorama por código...");
        MostrarSiguienteDiorama();
    }
}

    // FUNCIÓN DEL BOTÓN
    public void MostrarSiguienteDiorama()
    {
        Debug.Log($"--- CLIC EN BOTÓN SIGUIENTE --- (Índice antes del clic: {indiceActual})");

        if (dioramas.Length == 0) return;

        // Ocultamos el actual
        if (dioramas[indiceActual] != null)
        {
            Debug.Log($"Ocultando diorama actual: {dioramas[indiceActual].name}");
            dioramas[indiceActual].SetActive(false);
        }

        // Calculamos el siguiente
        int indiceAnterior = indiceActual;
        indiceActual = (indiceActual + 1) % dioramas.Length;
        
        Debug.Log($"Matemática de cambio: {indiceAnterior} + 1 calculado cíclicamente es = {indiceActual}");

        // Mostramos el nuevo
        if (dioramas[indiceActual] != null)
        {
            Debug.Log($"Mostrando nuevo diorama: {dioramas[indiceActual].name}");
            dioramas[indiceActual].SetActive(true);
            
            dioramas[indiceActual].transform.localScale = escalasIniciales[indiceActual];
            dioramas[indiceActual].transform.rotation = rotacionesIniciales[indiceActual];
        }

        ControladorSprite.ResetearBloqueoGlobal();
    }
}