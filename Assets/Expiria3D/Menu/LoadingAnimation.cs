using Expiria3DSpace;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    // Parámetros ajustables
    int numberOfSpheres = 6;          // Número de esferas en la animación
    float rotationSpeed = 50f;        // Velocidad de rotación en grados por segundo
    float baseDistance = 0.15f;       // Distancia base desde el centro
    float oscillationAmplitude = 0.015f; // Amplitud de la oscilación
    float oscillationFrequency = 1f;  // Frecuencia de oscilación
    float baseScale = 0.05f;          // Escala base de las esferas
    float sizeDecreaseFactor = 1f;    // Factor de disminución del tamaño de las esferas
    float transparencyDecreaseFactor = 1; // Factor de disminución de la transparencia
    Color sphereColor = Color.white;  // Color inicial de las esferas
    float centralCubeScale = 0.05f;   // Escala base del cubo central
    Color centralSphereColor = Color.white; // Color del cubo central

    float colorChangeSpeed = 0.2f;      // **Velocidad de cambio de color**

    private GameObject[] spheres;
    private float[] initialAngles;
    private Material[] sphereMaterials;       // **Array para almacenar los materiales de las esferas**
    private GameObject centralCube;
    private Material centralCubeMaterial;     // **Material del cubo central**
    private bool isPlaying = false;
    private float rotationSpeedRad;

    static LoadingAnimation instance;

    public static LoadingAnimation Instance
    {
        get
        {
            if (instance == null)
            {
                if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                {
                    instance = LoadModelFromURL.Instance.parentForImageTracker.transform.parent.GetComponentInChildren<LoadingAnimation>(true);
                }
                else
                {
                    instance = LoadModelFromURL.Instance.parentForWorldTracker.transform.parent.GetComponentInChildren<LoadingAnimation>(true);
                }

            }

            return instance;
        }
    }

    void Start()
    {
        rotationSpeedRad = rotationSpeed * Mathf.Deg2Rad;
        // Crear y configurar las esferas al iniciar el juego
        CreateSpheres();
    }

    void Update()
    {
        if (isPlaying)
        {
            float time = Time.time;

            // Obtener el ángulo de rotación del objeto padre en radianes
            float parentRotationAngle = transform.eulerAngles.y * Mathf.Deg2Rad;

            for (int i = 0; i < numberOfSpheres; i++)
            {
                GameObject sphere = spheres[i];
                float initialAngle = initialAngles[i];

                // Calcular el ángulo actual y agregar la rotación del padre
                float angle = initialAngle + rotationSpeedRad * time + parentRotationAngle;

                // Calcular la distancia radial sin oscilación
                float radialDistance = baseDistance;

                // Calcular la nueva posición
                float x = Mathf.Cos(angle) * radialDistance;
                float z = Mathf.Sin(angle) * radialDistance;
                sphere.transform.localPosition = new Vector3(x, 0, z);
            }

            // Rotar el cubo central
            centralCube.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

            // Calcular el factor de escala oscilante para el cubo central
            float scaleFactor = centralCubeScale + oscillationAmplitude * Mathf.Sin(time * oscillationFrequency * Mathf.PI * 0.5f);

            // Asegurarse de que el factor de escala no sea negativo o cero
            scaleFactor = Mathf.Max(scaleFactor, 0.01f); // Evitar que la escala sea cero o negativa

            // Aplicar el factor de escala al cubo central
            centralCube.transform.localScale = Vector3.one * scaleFactor;

            // **Actualizar el color de las esferas y el cubo central**
            float hue = (Time.time * colorChangeSpeed) % 1f;
            Color newColor = Color.HSVToRGB(hue, 1f, 1f);

            for (int i = 0; i < numberOfSpheres; i++)
            {
                sphereMaterials[i].color = newColor;
            }

            centralCubeMaterial.color = newColor;
        }
    }

    private void CreateSpheres()
    {
        // Crear y configurar las esferas
        spheres = new GameObject[numberOfSpheres];
        initialAngles = new float[numberOfSpheres];
        sphereMaterials = new Material[numberOfSpheres]; // **Inicializar el array de materiales**

        // Crear cubo central
        centralCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centralCube.transform.parent = this.transform;
        centralCube.transform.localPosition = Vector3.zero;
        centralCube.transform.localScale = Vector3.one * centralCubeScale;
        // Ajustar el color y la transparencia del cubo central
        Renderer centralRenderer = centralCube.GetComponent<Renderer>();
        centralCubeMaterial = new Material(Shader.Find("Standard"));
        centralCubeMaterial.color = centralSphereColor;
        centralRenderer.material = centralCubeMaterial;

        // Obtener el ángulo de rotación del objeto padre en radianes
        float parentRotationAngle = transform.eulerAngles.y * Mathf.Deg2Rad;

        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = this.transform;

            // Posicionar las esferas en círculo alrededor del centro
            float angle = i * Mathf.PI * 2 / numberOfSpheres;
            initialAngles[i] = angle;

            // Agregar la rotación del padre al ángulo
            float adjustedAngle = angle + parentRotationAngle;

            float x = Mathf.Cos(adjustedAngle) * baseDistance;
            float z = Mathf.Sin(adjustedAngle) * baseDistance;
            sphere.transform.localPosition = new Vector3(x, 0, z);

            // Ajustar el tamaño de las esferas
            float scale = baseScale * Mathf.Pow(sizeDecreaseFactor, i);
            sphere.transform.localScale = new Vector3(scale, scale, scale);

            // Ajustar el color y la transparencia
            Color adjustedColor = sphereColor;
            adjustedColor.a *= Mathf.Pow(transparencyDecreaseFactor, i);

            Renderer sphereRenderer = sphere.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));

            material.color = adjustedColor;
            material.SetFloat("_Mode", 3); // Modo transparente
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            sphereRenderer.material = material;

            // **Guardar referencia al material**
            sphereMaterials[i] = material;

            // Inicialmente desactivamos las esferas
            sphere.SetActive(false);

            spheres[i] = sphere;
        }

        // Inicialmente desactivar el cubo central
        centralCube.SetActive(false);
    }

    public void PlayLoading()
    {
        //if the gameObject is not active, return
        if (!this.gameObject.activeInHierarchy)
            return;

        // Si ya está reproduciendo, no hacer nada
        if (isPlaying)
            return;

        // Activar el cubo central
        centralCube.SetActive(true);

        // Activar las esferas
        foreach (GameObject sphere in spheres)
        {
            sphere.SetActive(true);
        }

        isPlaying = true;
    }

    public void StopLoading()
    {
        //if the gameObject is not active, return
        if (!this.gameObject.activeInHierarchy)
            return;

        // Detener la animación y desactivar las esferas
        isPlaying = false;

        centralCube.SetActive(false);

        foreach (GameObject sphere in spheres)
        {
            sphere.SetActive(false);
        }

        // Reiniciar la posición de las esferas
        for (int i = 0; i < numberOfSpheres; i++)
        {
            float angle = initialAngles[i];
            float x = Mathf.Cos(angle) * baseDistance;
            float z = Mathf.Sin(angle) * baseDistance;
            spheres[i].transform.localPosition = new Vector3(x, 0, z);
        }

        // Reiniciar la escala del cubo central
        centralCube.transform.localScale = Vector3.one * centralCubeScale;
    }
}