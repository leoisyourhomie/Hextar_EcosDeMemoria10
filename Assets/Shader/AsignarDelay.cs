using UnityEngine;

public class AsignarDelayUnico : MonoBehaviour
{
    // Contador estático centralizado
    private static int indiceObjeto = 0;

    void Start()
    {
        Renderer renderizador = GetComponent<Renderer>();
        if (renderizador != null)
        {
            MaterialPropertyBlock bloque = new MaterialPropertyBlock();
            
            // Incrementamos el índice del objeto actual
            indiceObjeto++;

            // 1. Matamos la coincidencia del ciclo del seno usando la proporción Áurea / número primo
            // Esto genera saltos de desfase matemáticamente asimétricos
            float delayUnico = (indiceObjeto * 2.399963f) % 6.283185f;
            
            // 2. Micro-variación de velocidad (entre -0.05 y +0.05) basada en su orden de aparición
            // Evita que dos objetos mantengan el mismo compás a lo largo del tiempo
            float modVelocidad = ((indiceObjeto * 7) % 10) * 0.01f - 0.05f;

            // Inyectamos ambos valores estáticos al bloque de propiedades
            bloque.SetFloat("_ObjetoDelay", delayUnico);
            bloque.SetFloat("_ObjetoSpeedMod", modVelocidad);
            
            renderizador.SetPropertyBlock(bloque);
        }
    }
}