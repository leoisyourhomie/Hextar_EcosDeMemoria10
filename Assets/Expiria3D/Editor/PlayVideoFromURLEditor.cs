namespace Expiria3DSpace
{
    using UnityEngine;
    using UnityEditor;
    using Expiria3DSpace.UGSService.Controller;
    using UnityEngine.Video;
    using UnityEngine.UI;

    [CustomEditor(typeof(PlayVideoFromURL))]
    public class PlayVideoFromURLEditor : Editor
    {
        PlayVideoFromURL playVideo;

        void OnEnable()
        {
            playVideo = (PlayVideoFromURL)target;
            // Ensure the material is unique to this object
            EnsureUniqueMaterial();
        }

        void EnsureUniqueMaterial()
        {
            //If the object is NOT in the scene (it is a prefab in the project view), we do not need to ensure the material is unique
            if (PrefabUtility.IsPartOfPrefabAsset(playVideo.gameObject))
            {
                return;
            }

            MeshRenderer meshRenderer = playVideo.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Get the current material
                Material mat = meshRenderer.sharedMaterial;

                if (mat != null)
                {
                    // Check if the material is an asset
                    if (mat.name == "SampleVideo" || mat.name == "SampleVideoChroma")
                    {
                        // Duplicate the material to create a unique instance
                        Material newMat = new Material(mat);
                        newMat.name = "Video_" + Random.Range(0, int.MaxValue);

                        // Save the new material as an asset
                        SaveMaterialAsset(newMat);

                        // Assign the new material to the renderer
                        meshRenderer.sharedMaterial = newMat;
                    }
                    else
                    {
                        // The material is already a unique instance
                        // But we need to check if other objects are using it
                        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

                        foreach (MeshRenderer renderer in renderers)
                        {
                            if (renderer == meshRenderer) continue; // Skip the current renderer

                            if (renderer.sharedMaterial == mat)
                            {
                                // Another object is using the same material instance
                                // Duplicate the material
                                Material newMat = new Material(mat);
                                newMat.name = "Video_" + Random.Range(0, int.MaxValue);

                                // Save the new material as an asset
                                SaveMaterialAsset(newMat);

                                // Assign the new material to the renderer
                                meshRenderer.sharedMaterial = newMat;
                                break;
                            }
                        }
                    }
                }
            }
        }

        void SaveMaterialAsset(Material material)
        {
            string materialFolder = "Assets/VideoMaterials";

            // Ensure the folder exists
            if (!AssetDatabase.IsValidFolder(materialFolder))
            {
                AssetDatabase.CreateFolder("Assets", "VideoMaterials");
            }

            // Generate a unique asset path
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{materialFolder}/{material.name}.mat");

            // Create the new material asset
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void DrawTitle()
        {
            Texture openUrlIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/ar_video.png");

            EditorGUILayout.BeginHorizontal();
            {
                //draw icon
                GUILayout.Label(openUrlIcon, GUILayout.Width(30), GUILayout.Height(30));

                //flexible vertical
                EditorGUILayout.BeginVertical(GUILayout.Height(30));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Video", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 15, wordWrap = true });
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = Color.cyan;
                //icon CloudConnect
                Texture cloudConnectIcon = EditorGUIUtility.IconContent("CloudConnect").image;
                if (GUILayout.Button(new GUIContent(" Subir Un Video", cloudConnectIcon), ArActionsEditorTools.BigButtonStyle, GUILayout.Height(35), GUILayout.Width(175)))
                {
                    Application.OpenURL("https://expiria3d.com/user/storage");
                }

                GUI.backgroundColor = Color.white;

            }
            EditorGUILayout.EndHorizontal();
        }

        public void SetMaterial(MeshRenderer meshRenderer, Material materialFromAsset)
        {
            if (meshRenderer == null || materialFromAsset == null)
            {
                return;
            }

            // Create a new instance of the material
            Material materialInstance = new Material(materialFromAsset);
            materialInstance.name = "Video_" + Random.Range(0, int.MaxValue);

            // Save the new material as an asset
            SaveMaterialAsset(materialInstance);

            meshRenderer.sharedMaterial = materialInstance;

            // Ensure the material is unique (this may not be necessary now, but kept for consistency)
            EnsureUniqueMaterial();
        }

        public override void OnInspectorGUI()
        {
            if (!UGSServiceManager.UserValidation.ValidateUser(false)) return; EditorGUI.BeginChangeCheck();
            using (new GUILayout.VerticalScope("groupbox", GUILayout.Height(50)))
            {
                DrawTitle();
                GUILayout.Space(10);
                playVideo.videoURL = EditorGUILayout.TextField("Video URL", playVideo.videoURL);
                playVideo.videoURL = playVideo.videoURL.Trim();
                playVideo.loadingPanel = (GameObject)EditorGUILayout.ObjectField("Indicador de carga", playVideo.loadingPanel, typeof(GameObject), true);
                playVideo.playOnStart = EditorGUILayout.Toggle("Reproducir al Iniciar", playVideo.playOnStart);
                playVideo.pauseAndPlayOnVideoClick = EditorGUILayout.Toggle("Pausar/Reproducir al Hacer Click", playVideo.pauseAndPlayOnVideoClick);
                playVideo.loop = EditorGUILayout.Toggle("Repetir Al Terminar", playVideo.loop);
                playVideo.videoVolume = EditorGUILayout.Slider("Volumen", playVideo.videoVolume, 0, 1);
                playVideo.playbackSpeed = EditorGUILayout.FloatField("Velocidad", playVideo.playbackSpeed);

                if (playVideo.playbackSpeed < 0.1f)
                {
                    playVideo.playbackSpeed = 0.1f;
                }

                if (playVideo.playbackSpeed > 4)
                {
                    playVideo.playbackSpeed = 4;
                }

                if (playVideo.GetComponent<MeshRenderer>() != null)
                {
                    Material[] materials = playVideo.GetComponent<MeshRenderer>().sharedMaterials;

                    if (materials.Length > 1)
                    {
                        EditorGUILayout.HelpBox("Error: El MeshRenderer tiene más de 1 material", MessageType.Error);
                    }
                    else if (materials.Length == 1)
                    {
                        Material material = materials[0];

                        if (material == null)
                        {
                            //Set the default material
                            Material materialFromAsset = AssetDatabase.LoadAssetAtPath<Material>("Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideo.mat");

                            if (materialFromAsset == null)
                            {
                                Debug.LogError("Material not found at path: Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideo.mat");
                            }
                            else
                            {
                                SetMaterial(playVideo.GetComponent<MeshRenderer>(), materialFromAsset);
                            }

                            material = playVideo.GetComponent<MeshRenderer>().sharedMaterial;
                        }

                        bool isChromaKey = material.shader.name == "Expiria3D/ChromaKey";

                        bool prevIsChromaKey = isChromaKey;
                        isChromaKey = EditorGUILayout.Toggle("Remover Fondo ", isChromaKey);

                        string materialPath = isChromaKey
                        ? "Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideoChroma.mat"
                        : "Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideo.mat";

                        if (prevIsChromaKey != isChromaKey)
                        {
                            Material materialFromAsset = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                            if (materialFromAsset == null)
                            {
                                Debug.LogError("Material not found at path: " + materialPath);
                            }
                            else
                            {
                                // Create a new instance of the material
                                Material materialInstance = new Material(materialFromAsset);
                                materialInstance.name = "Video_" + Random.Range(0, int.MaxValue);

                                // Save the new material as an asset
                                SaveMaterialAsset(materialInstance);

                                playVideo.GetComponent<MeshRenderer>().sharedMaterial = materialInstance;

                                // Ensure the material is unique (this may not be necessary now, but kept for consistency)
                                EnsureUniqueMaterial();
                            }
                        }

                        //chroma key color picker
                        //The properties are:
                        //MaskCol ("Color", Color)  = (0, 255, 0.0, 1.0)
                        //_Sensitivity ("Umbral", Range(0,1)) = 0.15
                        //_Smooth ("Suavizado", Range(0,1)) = 0.01
                        //The shader is in Assets/Expiria3D/Video/Shaders/ChromaKey.shader and the name is Expiria3D/ChromaKey
                        if (material.shader.name == "Expiria3D/ChromaKey") //we do not use the variable isChromaKey because it takes like 1 frame to update
                        {
                            Color maskCol = material.GetColor("_MaskCol");
                            float sensitivity = material.GetFloat("_Sensitivity");
                            float smooth = material.GetFloat("_Smooth");

                            maskCol = EditorGUILayout.ColorField("Color", maskCol);
                            sensitivity = EditorGUILayout.Slider("Umbral", sensitivity, 0, 1);
                            smooth = EditorGUILayout.Slider("Suavizado", smooth, 0, 1);

                            material.SetColor("_MaskCol", maskCol);
                            material.SetFloat("_Sensitivity", sensitivity);
                            material.SetFloat("_Smooth", smooth);
                        }

                        if (material.shader.name == "Expiria3D/ChromaKey"
                            || material.shader.name == "Expiria3D/TextureDoubleSide")
                        {
                            GUILayout.Space(5);

                            //_CullMode
                            //0: Off
                            //1: Front
                            //2: Back

                            int cullMode = material.GetInt("_CullMode");
                            cullMode = EditorGUILayout.Popup("Visualización ", cullMode, new string[] { "Mostrar por ambos lados", "Mostrar en la parte de atrás", "Mostrar en el frente" });

                            material.SetInt("_CullMode", cullMode);

                            //mark the material as dirty
                            EditorUtility.SetDirty(material);
                        }
                    }
                    else
                    {
                        //add a material automatically
                        Material materialFromAsset = AssetDatabase.LoadAssetAtPath<Material>("Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideo.mat");

                        if (materialFromAsset == null)
                        {
                            Debug.LogError("Material not found at path: Assets/Expiria3D/Samples/VideoTextures/Materials/SampleVideo.mat");
                        }
                        else
                        {
                            SetMaterial(playVideo.GetComponent<MeshRenderer>(), materialFromAsset);
                        }

                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Error: MeshRenderer no encontrado", MessageType.Error);
                }

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Calidad del video al reproducir en Web", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = Color.white;

                if (playVideo.textureWidth == 320 && playVideo.textureHeight == 240)
                {
                    GUI.backgroundColor = Color.green;
                }

                if (GUILayout.Button("Baja", EditorStyles.miniButtonLeft))
                {
                    playVideo.textureWidth = 320;
                    playVideo.textureHeight = 240;
                }

                GUI.backgroundColor = Color.white;

                if (playVideo.textureWidth == 640 && playVideo.textureHeight == 480)
                {
                    GUI.backgroundColor = Color.green;
                }

                if (GUILayout.Button("Media", EditorStyles.miniButtonMid))
                {
                    playVideo.textureWidth = 640;
                    playVideo.textureHeight = 480;
                }

                GUI.backgroundColor = Color.white;

                if (playVideo.textureWidth == 1280 && playVideo.textureHeight == 720)
                {
                    GUI.backgroundColor = Color.green;
                }

                if (GUILayout.Button("Alta", EditorStyles.miniButtonMid))
                {
                    playVideo.textureWidth = 1280;
                    playVideo.textureHeight = 720;
                }

                GUI.backgroundColor = Color.white;

                if (playVideo.textureWidth == 1920 && playVideo.textureHeight == 1080)
                {
                    GUI.backgroundColor = Color.green;
                }

                if (GUILayout.Button("Muy Alta", EditorStyles.miniButtonRight))
                {
                    playVideo.textureWidth = 1920;
                    playVideo.textureHeight = 1080;
                }

                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Ancho: " + playVideo.textureWidth + ", Alto: " + playVideo.textureHeight);


                GUILayout.Space(10);

                EditorGUILayout.LabelField("Controles", EditorStyles.boldLabel);
                playVideo.showPlayButton = EditorGUILayout.Toggle("Mostrar Botón Play/Pause", playVideo.showPlayButton);

                if (playVideo.showPlayButton)
                {
                    playVideo.playButton = (Button)EditorGUILayout.ObjectField("Botón de Play", playVideo.playButton, typeof(Button), true);
                    playVideo.pauseButton = (Button)EditorGUILayout.ObjectField("Botón de Pausa", playVideo.pauseButton, typeof(Button), true);
                }

                playVideo.showProgressBar = EditorGUILayout.Toggle("Mostrar Barra de Progreso", playVideo.showProgressBar);

                if (playVideo.showProgressBar)
                {
                    playVideo.progressBar = (Slider)EditorGUILayout.ObjectField("Barra de Progreso", playVideo.progressBar, typeof(Slider), true);
                }
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying)
                {
                    if (playVideo.GetComponent<VideoPlayer>() != null)
                    {
                        playVideo.GetComponent<VideoPlayer>().isLooping = playVideo.loop;
                        playVideo.GetComponent<VideoPlayer>().playbackSpeed = playVideo.playbackSpeed;
                    }
                }

                if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(target)
                    && UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null)
                {
                    EditorUtility.SetDirty(target);
                }
            }

            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(playVideo, "Undo playVideo");
        }
    }
}
