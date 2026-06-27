using UnityEngine;

public class ControladorShader : MonoBehaviour
{
    [Header("Configuración del Brillo")]
    [Tooltip("Asigna aquí el material que tiene tu shader aditivo original (LuminanceObject)")]
    public Material materialGlow;

    [Header("Ajuste de Escala (Móviles/RA)")]
    [Range(1.0f, 1.05f)]
    public float grosorDelBrillo = 1.002f; 

    // Almacenamos los valores aquí para que todo el helicóptero comparta los mismos datos de tiempo
    private float delaySincronizado;
    private float velocidadSincronizada;

    void Start()
    {
        if (materialGlow == null)
        {
            Debug.LogError("Por favor, asigna el material de efecto en el Inspector.");
            return;
        }

        // CALCULAMOS UNA SOLA VEZ: Generamos el desfase inicial del objeto completo
        delaySincronizado = Random.Range(0f, 5f);
        velocidadSincronizada = Random.Range(-0.1f, 0.1f);

        // CASO 1: Mallas estáticas
        MeshFilter[] filtrosMalla = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filtro in filtrosMalla)
        {
            Renderer rendOriginal = filtro.GetComponent<Renderer>();
            if (rendOriginal != null && rendOriginal.gameObject.name != "Capa_Glow_Efecto")
            {
                CrearClonEstatic(filtro, rendOriginal.sharedMaterials.Length);
            }
        }

        // CASO 2: Mallas animadas
        SkinnedMeshRenderer[] renderersAnimados = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinned in renderersAnimados)
        {
            if (skinned.gameObject.name != "Capa_Glow_Efecto")
            {
                CrearClonAnimado(skinned);
            }
        }
    }

    void CrearClonEstatic(MeshFilter filtro, int cantidadMateriales)
    {
        GameObject clonGlow = new GameObject("Capa_Glow_Efecto");
        clonGlow.transform.SetParent(filtro.transform, false);
        clonGlow.transform.localScale = Vector3.one * grosorDelBrillo;

        MeshFilter nuevoFiltro = clonGlow.AddComponent<MeshFilter>();
        nuevoFiltro.sharedMesh = filtro.sharedMesh;

        MeshRenderer nuevoRenderer = clonGlow.AddComponent<MeshRenderer>();
        AplicarMaterialesAlRenderer(nuevoRenderer, cantidadMateriales);
    }

    void CrearClonAnimado(SkinnedMeshRenderer original)
    {
        GameObject clonGlow = new GameObject("Capa_Glow_Efecto");
        clonGlow.transform.SetParent(original.transform, false);
        clonGlow.transform.localScale = Vector3.one * grosorDelBrillo;

        SkinnedMeshRenderer nuevoSkinned = clonGlow.AddComponent<SkinnedMeshRenderer>();
        nuevoSkinned.sharedMesh = original.sharedMesh;
        nuevoSkinned.bones = original.bones;
        nuevoSkinned.rootBone = original.rootBone;
        nuevoSkinned.localBounds = original.localBounds;

        AplicarMaterialesAlRenderer(nuevoSkinned, original.sharedMaterials.Length);
    }

    void AplicarMaterialesAlRenderer(Renderer nuevoRenderer, int cantidad)
    {
        Material[] arrayMaterialesEfecto = new Material[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            // Creamos la instancia para no interferir con otros objetos de la escena
            Material matInstancia = new Material(materialGlow);
            
            // INYECCIÓN SINCRONIZADA: Todos los materiales del modelo adoptan exactamente los mismos valores
            matInstancia.SetFloat("_ObjetoDelay", delaySincronizado);
            matInstancia.SetFloat("_ObjetoSpeedMod", velocidadSincronizada);
            
            arrayMaterialesEfecto[i] = matInstancia;
        }
        nuevoRenderer.materials = arrayMaterialesEfecto;
    }
}