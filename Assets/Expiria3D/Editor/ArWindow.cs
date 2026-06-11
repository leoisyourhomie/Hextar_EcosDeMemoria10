namespace Expiria3DSpace
{
    using Expiria3DSpace.UGSService.Controller;
    using Expiria3DSpace.World.Demo;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    public class ArWindow : EditorWindow
    {
        Vector2 scrollPos;
        Texture sampleIcon;
        static Texture windowIcon;
        static Texture expiria3dIcon;

        GUIStyle textStyle = null;
        GUIStyle itemStyle = null;

        Texture background;

        public static bool AdvancedMode
        {
            get
            {
                return EditorPrefs.GetBool("UGS_AdvancedMode", false);
            }
            set
            {
                EditorPrefs.SetBool("UGS_AdvancedMode", value);
            }
        }

        struct Item
        {
            public string name;
            public string iconName;
            public string prefabName;
            public bool showPrefabThumbnail;

            public Item(string name, string iconName, string prefabName, bool showPrefabThumbnail)
            {
                this.name = name;
                this.iconName = iconName;
                this.prefabName = prefabName;
                this.showPrefabThumbnail = showPrefabThumbnail;
            }
        }

        struct Category
        {
            public string name;
            public string iconName;

            public List<Item> items;

            public Category(string name, string iconName, List<Item> items)
            {
                this.name = name;
                this.iconName = iconName;
                this.items = items;

            }
        }

        List<Category> categories = new List<Category>()
        {
            new Category("Configuración General", "ar_settings.png", new List<Item>()),
            new Category("Elementos", "ar_text.png", new List<Item>()
            {
                new Item("Texto", "ar_text.png", "Text", false),
                new Item("Imagen", "ar_img.png", "Image", false),
                new Item("Botón Con Texto", "ar_btn_txt.png", "ButtonWithText", false),
                new Item("Botón Con Imagen", "ar_btn_img.png", "ButtonWithImage", false),
                new Item("Video", "ar_video_normal.png", "SampleVideo", false)
            }),
           new Category("Iconos 3D", "ar_box.png", new List<Item>()
            {
                new Item("Arrow", "", "Arrow", true),
                new Item("Arrow2", "", "Arrow2", true),
                new Item("BlackPhone", "", "BlackPhone", true),
                new Item("Bolt", "", "Bolt", true),
                new Item("Call", "", "Call", true),
                new Item("CallEnd", "", "CallEnd", true),
                new Item("CallEnd2", "", "CallEnd2", true),
                new Item("Discord", "", "Discord", true),
                new Item("Facebook", "", "Facebook", true),
                new Item("Google", "", "Google", true),
                new Item("GoogleBoard", "", "GoogleBoard", true),
                new Item("IncomeCall", "", "IncomeCall", true),
                new Item("Instagram", "", "Instagram", true),
                new Item("Instagram2", "", "Instagram2", true),
                new Item("MapPointer", "", "MapPointer", true),
                new Item("MobilePhone", "", "MobilePhone", true),
                new Item("OutcomeCall", "", "OutcomeCall", true),
                new Item("PhoneRing", "", "PhoneRing", true),
                new Item("Pinterest", "", "Pinterest", true),
                new Item("Power", "", "Power", true),
                new Item("Telegram", "", "Telegram", true),
                new Item("TikTok", "", "TikTok", true),
                new Item("TikTok2", "", "TikTok2", true),
                new Item("Twitch", "", "Twitch", true),
                new Item("TwitterX", "", "TwitterX", true),
                new Item("WhatsApp", "", "WhatsApp", true),
                new Item("WhatsApp2", "", "WhatsApp2", true),
                new Item("WhatsApp3", "", "WhatsApp3", true),
                new Item("YouTube1", "", "YouTube1", true),
                new Item("YouTube2", "", "YouTube2", true),
                new Item("YouTube3", "", "YouTube3", true),
                new Item("YouTube4", "", "YouTube4", true),
            }),

            new Category("Buscar Contenido", "ar_web.png", new List<Item>()),
        };

        Dictionary<string, Texture> icons = new Dictionary<string, Texture>();
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        string prefabSamplesPath = "Assets/Expiria3D/Samples/Prefabs";

        //URL, name, icon, type
        public List<(string, string, string, string)> resources = new List<(string, string, string, string)>()
        {
            ("https://sketchfab.com/search?features=downloadable&licenses=322a749bcfa841b29dff1e8a1bb74b0b&licenses=b9ddc40b93e34cdca1fc152f39b9f375&licenses=72360ff1740d419791934298b8b6d270&licenses=bbfe3f7dbcdd4122b966b85b9786a989&licenses=2628dbe5140a4e9592126c8df566c0b7&licenses=34b725081a6a4184957efaec2cb84ed3&licenses=7c23a1ba438d4306920229c12afcb5f9&licenses=72eb2b1960364637901eacce19283624&type=models", "Sketchfab", "ar_box.png", "Modelos 3D"),
            ("https://poly.pizza/", "Poly Pizza", "ar_box.png", "Modelos 3D"),
            ("https://text2stl.mestres.fr/en-us/generator", "Generar Texto 3D", "ar_text.png", "Modelos 3D"),
            ("https://avaturn.me/", "Avatarun", "ar_animation.png", "Tu Avatar en 3D"),
            //https://www.freepik.es/search?format=search&iconType=standard&last_filter=iconType&last_value=standard&type=icon
            ("https://www.freepik.es/search?format=search&iconType=standard&last_filter=iconType&last_value=standard&type=icon", "Freepik", "ar_img.png", "Iconos"),
            ("https://www.freepik.es/search?format=search&last_filter=selection&last_value=1&selection=1&type=photo", "Freepik", "ar_img.png", "Fotos e imágenes"),
            ("https://fonts.google.com/", "Google Fonts", "ar_text.png", "Fuentes de Texto"),
            ("https://www.dafont.com/", "Dafont", "ar_text.png", "Fuentes de Texto"),
            ("https://freepd.com/", "FreePD", "ar_sound.png", "Sonidos"),
            ("http://dig.ccmixter.org/games", "CCMixter", "ar_sound.png", "Sonidos"),
            ("https://musopen.org/music/", "Musopen", "ar_sound.png", "Sonidos"),
            ("https://freesound.org/", "Freesound", "ar_sound.png", "Sonidos"),
        };


        List<string> componentsToDisableIconInScene = new List<string>()
        {
            "ArAction", "TextMeshPro", "TextMeshProUGUI", "Capa", "PlayVideoFromURL"
        };

        void SetGizmoToComponent(string componentName, bool value)
        {
            var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var ClassId = Annotation.GetField("classID");
            var ScriptClass = Annotation.GetField("scriptClass");

            System.Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Array annotations = (Array)GetAnnotations?.Invoke(null, null);

            foreach (var a in annotations)
            {
                int classId = (int)ClassId.GetValue(a);
                string scriptClass = (string)ScriptClass.GetValue(a);
                if (scriptClass == componentName)
                {
                    SetIconEnabled?.Invoke(null, new object[] { classId, scriptClass, (value) ? 1 : 0 });
                    break;
                }
            }
        }


        bool IsFoldout(string name)
        {
            return EditorPrefs.GetBool("ShowCompInfo-" + name, false);
        }

        void SetFoldout(string name, bool value)
        {
            EditorPrefs.SetBool("ShowCompInfo-" + name, value);
        }

        Texture GetIcon(string iconName, bool isAssetPreview, bool isUGameEditorIcon = false)
        {
            if (!icons.ContainsKey(iconName))
            {
                if (isAssetPreview)
                {
                    return AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<GameObject>(prefabSamplesPath + "/Model Prefab/" + iconName + ".prefab")); //this cannot be cached because unity takes a little to load the texture
                }
                else
                {
                    if (isUGameEditorIcon)
                    {
                        icons.Add(iconName, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/" + iconName));
                    }
                    else
                    {
                        icons.Add(iconName, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/" + iconName));
                    }
                }
            }

            Texture icon = icons[iconName];
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/ar_web.png");
            }

            return icon;
        }

        [MenuItem("Expiria3D/Abrir Ventana")]
        public static void OpenWindow()
        {
            ShowWindow();
        }

        public static void ShowWindow()
        {
            ArWindow window = EditorWindow.GetWindow<ArWindow>();

            windowIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/ar.png");

            if (expiria3dIcon == null)
                expiria3dIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/logo_expiria_sm.png");

            // Create the instance of GUIContent to assign to the window. Gives the title and the icon
            GUIContent titleContent = new GUIContent("Expiria3D", windowIcon);
            window.titleContent = titleContent;

            window.Show();
        }

        void OnGUI()
        {
            if (!UGSServiceManager.UserValidation.ValidateUser(false)) return;

            if (arManager == null)
            {
                arManager = FindFirstObjectByType<ARManager>();
            }

            DrawEditor(this.position.width, this.position.height);
        }

        ARManager arManager = null;

        void OnEnable()
        {
            InstallGltfAndTMPProPackages.Instance.Initialize();
            //find the ARManager, if it exists, find all the image trackers.
            //after that, check if all the image trackers are in the list of image trackers of the ARManager. If not, add them.
            arManager = FindFirstObjectByType<ARManager>();

            if (arManager != null)
            {
                ImageTracker[] imageTrackers = FindObjectsByType<ImageTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                for (int i = 0; i < imageTrackers.Length; i++)
                {
                    if (!arManager.imageTrackers.Contains(imageTrackers[i]))
                    {
                        arManager.imageTrackers.Add(imageTrackers[i]);

                        //set the index
                        imageTrackers[i].targetIndex = arManager.imageTrackers.Count - 1;

                        EditorUtility.SetDirty(arManager);
                        EditorUtility.SetDirty(imageTrackers[i]);
                    }
                }
            }

            foreach (string componentName in componentsToDisableIconInScene)
            {
                SetGizmoToComponent(componentName, false);
            }
        }

        public void DrawEditor(float windowWidth, float windowHeight)
        {
            if (background == null)
                background = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/bg.png");

            //if (DragAndDrop.objectReferences.Length == 0 && uGameActionContentDragging != null)
            //{
            //    uGameActionContentDragging = null;
            //}

            //Variables for scroll
            float width = windowWidth;
            float height = windowHeight;

            GUI.backgroundColor = Color.white;

            textStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, -10, 0),
                fontSize = 12,
                fixedHeight = 25,
                wordWrap = true
            };

            //Start scroll view
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(width), GUILayout.Height(height));
            //Draw the background
            if (arManager != null)
                GUI.DrawTexture(new Rect(0, 0, windowWidth, 100), background, ScaleMode.StretchToFill);
            //-------------------------------------------------------------------------------------- 
            Content(windowWidth);
            //-------------------------------------------------------------------------------------- /
            //End scroll view
            EditorGUILayout.EndScrollView();
            //-------------------------------------------------------------------------------------- /
        }

        //public static void LoadLayout()
        //{
        //    ShowWindow();

        //    // Ruta al archivo de layout guardado
        //    string layoutPath = "Assets/Expiria3D/LayoutAR.wlt";

        //    //if the file in Assets/Expiria3D/LayoutAR.wlt DOES NOT exist, return 
        //    if (File.Exists("Assets/Expiria3D/Editor/Expiria3D_Editor.dll"))
        //    {
        //        layoutPath = "Assets/Expiria3D/LayoutAR_Editor.wlt";

        //        if (!File.Exists(layoutPath))
        //        {
        //            Debug.LogWarning("El archivo de layout no se encontró en la ruta: Assets/Expiria3D/LayoutAR_Editor.wlt");
        //            return;
        //        }
        //    }

        //    // Convertir la ruta relativa a absoluta
        //    string fullPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), layoutPath);

        //    // Verificar si el archivo existe
        //    if (!System.IO.File.Exists(fullPath))
        //    {
        //        Debug.LogError($"El archivo de layout no se encontró en la ruta: {fullPath}");
        //        return;
        //    }

        //    // Obtener el tipo de la clase interna WindowLayout
        //    System.Type windowLayoutType = typeof(Editor).Assembly.GetType("UnityEditor.WindowLayout");

        //    if (windowLayoutType != null)
        //    {
        //        // Obtener el método estático LoadWindowLayout
        //        MethodInfo loadWindowLayoutMethod = windowLayoutType.GetMethod(
        //            "LoadWindowLayout",
        //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
        //            null,
        //            new System.Type[] { typeof(string), typeof(bool) },
        //            null
        //        );

        //        if (loadWindowLayoutMethod != null)
        //        {
        //            try
        //            {
        //                // Invocar el método para cargar el layout
        //                loadWindowLayoutMethod.Invoke(null, new object[] { fullPath, false });
        //            }
        //            catch (System.Exception ex)
        //            {
        //                Debug.LogError($"Error al cargar el layout: {ex.Message}");
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogError("No se pudo encontrar el método LoadWindowLayout.");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("No se pudo encontrar el tipo UnityEditor.WindowLayout.");
        //    }
        //}

        private void RemoveDeletedScenesFromBuildSettings()
        {
            // Obtener todas las escenas del Build Settings
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Lista para almacenar las escenas válidas
            var validScenes = new List<EditorBuildSettingsScene>();

            foreach (var scene in scenes)
            {
                if (System.IO.File.Exists(scene.path))
                {
                    // Si el archivo de la escena existe, la añadimos a la lista de escenas válidas
                    validScenes.Add(scene);
                }
            }

            // Actualizar los Build Settings con las escenas válidas
            EditorBuildSettings.scenes = validScenes.ToArray();

            // Guardar los Build Settings
            AssetDatabase.SaveAssets();
        }

        void CreateImgScene()
        {
            if (GUILayout.Button("Crear escena para Seguimiento de Imagen", ArActionsEditorTools.BigButtonStyle))
            {
                // Ruta de la escena plantilla dentro de tu proyecto
                string templatePath = "Assets/Expiria3D/Scenes/IMAGE_TRACKER_TEMPLATE.unity";

                // Panel para que el usuario escoja dónde guardar la copia
                string savePath = EditorUtility.SaveFilePanel(
                    "Crear escena para Seguimiento de Imagen",
                    Application.dataPath,
                    "NuevaEscena",
                    "unity"
                );

                // Si no se cancela el guardado
                if (!string.IsNullOrEmpty(savePath))
                {
                    // Verificamos si la ruta está dentro de "Assets"
                    if (savePath.StartsWith(Application.dataPath))
                    {
                        // Construimos la ruta relativa a "Assets"
                        string relativePath = "Assets" + savePath.Substring(Application.dataPath.Length);

                        FileUtil.CopyFileOrDirectory(templatePath, relativePath);
                        AssetDatabase.Refresh();

                        // Enfocar la escena en el Project Window
                        var newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(relativePath);
                        if (newSceneAsset != null)
                        {
                            EditorGUIUtility.PingObject(newSceneAsset);
                            // Abrir la escena
                            EditorSceneManager.OpenScene(relativePath, OpenSceneMode.Single);
                            AddCurrentSceneToBuildSettings(false);

                            ArTargetsLoader.LoadImagesAndType();
                            //repaint the window
                            Repaint();
                        }
                    }
                    else
                    {
                        // Mostrar el mensaje de error
                        EditorUtility.DisplayDialog(
                            "Error",
                            "Debe seleccionar una carpeta dentro de 'Assets' para crear la escena.",
                            "Aceptar"
                        );
                    }
                }
            }
        }
        void CreateWorldScene()
        {
            if (GUILayout.Button("Crear escena para Seguimiento de Mundo", ArActionsEditorTools.BigButtonStyle))
            {
                // Ruta de la escena plantilla dentro de tu proyecto
                string templatePath = "Assets/Expiria3D/Scenes/WORLD_TRACKER_TEMPLATE.unity";

                // Panel para que el usuario escoja dónde guardar la copia
                string savePath = EditorUtility.SaveFilePanel(
                    "Crear escena para Seguimiento de Mundo",
                    Application.dataPath,
                    "NuevaEscena",
                    "unity"
                );

                // Si no se cancela el guardado
                if (!string.IsNullOrEmpty(savePath))
                {
                    // Verificamos si la ruta está dentro de "Assets"
                    if (savePath.StartsWith(Application.dataPath))
                    {
                        // Construimos la ruta relativa a "Assets"
                        string relativePath = "Assets" + savePath.Substring(Application.dataPath.Length);

                        FileUtil.CopyFileOrDirectory(templatePath, relativePath);
                        AssetDatabase.Refresh();

                        // Enfocar la escena en el Project Window
                        var newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(relativePath);
                        if (newSceneAsset != null)
                        {
                            EditorGUIUtility.PingObject(newSceneAsset);
                            // Abrir la escena
                            EditorSceneManager.OpenScene(relativePath, OpenSceneMode.Single);
                            AddCurrentSceneToBuildSettings(false);

                            ArTargetsLoader.LoadImagesAndType();
                            //repaint the window
                            Repaint();
                        }
                    }
                    else
                    {
                        // Mostrar el mensaje de error
                        EditorUtility.DisplayDialog(
                            "Error",
                            "Debe seleccionar una carpeta dentro de 'Assets' para crear la escena.",
                            "Aceptar"
                        );
                    }
                }
            }
        }

        public void Content(float windowWidth)
        {
            if (expiria3dIcon == null)
            {
                expiria3dIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/logo_expiria_sm.png");
            }

            GUIStyle styleTxt = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 15, wordWrap = true };

            GUIStyle btnStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                wordWrap = true,
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(0, 0, 0, 0),
                fontSize = 15,
                fixedHeight = 40
            };

            //If unity is compiling, show a warning
            if (BuildPipeline.isBuildingPlayer)
            {
                ArActionsEditorTools.WarningWithBigFont("Por favor, espere a que Unity termine de compilar.\r\nEsto puede tardar unos minutos.");

                GUIUtility.ExitGUI();
                return;
            }

            if (InstallGltfAndTMPProPackages.Instance.InstallIfNeeded())
            {
                ArActionsEditorTools.WarningWithBigFont("Instalando paquetes necesarios...\r\nNo cierre esta ventana hasta que termine la instalación. Esto puede tardar unos minutos.");

                Repaint();
                GUIUtility.ExitGUI();
                return;
            }

            ARManager arManager = FindFirstObjectByType<ARManager>();
            BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

            if (arManager == null)
            {
                ArActionsEditorTools.WarningWithBigFont("Esta no es una escena de Realidad Aumentada. Para empezar puede crear una nueva escena desde los botones de abajo, o abra una de las escenas que ya tenga en su proyecto.");
                EditorGUILayout.BeginHorizontal();

                CreateImgScene();
                CreateWorldScene();
                EditorGUILayout.EndHorizontal();

                //if (GUILayout.Button("Configurar Para Realidad Aumentada", ArActionsEditorTools.BigButtonStyle))
                //{
                //    ARSceneUtils.CreateARManager();

                //    if (EditorSceneManager.GetActiveScene().name == "")
                //        EditorSceneManager.SaveOpenScenes();

                //    if (!ThisSceneExistInBuildSettings())
                //    {
                //        AddCurrentSceneToBuildSettings(false);
                //    }

                //    if (activeBuildTarget != BuildTarget.WebGL)
                //    {
                //        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                //    }

                //    //Set light position to 0,1,0
                //    Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

                //    foreach (Light light in lights)
                //    {
                //        if (light.type == LightType.Directional)
                //        {
                //            light.transform.rotation = Quaternion.Euler(50, -30, 0);
                //            light.transform.position = new Vector3(0, 1, 0);
                //        }
                //    }
                //}

                GUIUtility.ExitGUI();
                return;
            }

            if (activeBuildTarget != BuildTarget.WebGL)
            {
                ArActionsEditorTools.WarningWithBigFont("La plataforma de destino no es WebGL. Por favor, cambie la plataforma de destino a WebGL para continuar.");

                if (GUILayout.Button("Cambiar a WebGL", ArActionsEditorTools.BigButtonStyle))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                }

                GUIUtility.ExitGUI();
                return;
            }

            if (EditorSceneManager.GetActiveScene().name == "")
            {
                ArActionsEditorTools.WarningWithBigFont("Por favor, guarde la escena antes de continuar.");

                if (GUILayout.Button("Guardar Escena", ArActionsEditorTools.BigButtonStyle))
                {
                    EditorSceneManager.SaveOpenScenes();
                }

                GUIUtility.ExitGUI();
                return;
            }

            if (!ThisSceneExistInBuildSettings())
            {
                ArActionsEditorTools.WarningWithBigFont("Esta escena no está agregada en la lista de escenas a exportar. Por favor, agréguela para continuar.");

                if (GUILayout.Button("Agregar Escena", ArActionsEditorTools.BigButtonStyle))
                {
                    AddCurrentSceneToBuildSettings();
                }

                GUIUtility.ExitGUI();
                return;
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(5);

            GUILayout.Label(expiria3dIcon, GUILayout.Width(180), GUILayout.Height(50));

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUILayout.Width(80));
            Texture webIcon = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small").image;
            Texture iconUpload = EditorGUIUtility.IconContent("UpArrow").image;

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button(new GUIContent(" Publicar", iconUpload), ArActionsEditorTools.BigButtonStyle, GUILayout.Width(150), GUILayout.Height(33)))
            {
                //foreach ImageTracker with show in web disabled, set null to the target
                ImageTracker[] imageTrackers = FindObjectsByType<ImageTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                //Set null to the target of the image trackers that are not going to be shown in the web to avoid exporting the image
                foreach (ImageTracker imageTracker in imageTrackers)
                {
                    if (!imageTracker.showTrackingImageInWeb)
                    {
                        Transform t = imageTracker.transform.Find("_TargetTrackingImagePreview");

                        if (t != null)
                        {
                            Image img = t.GetComponent<Image>();
                            if (img != null)
                            {
                                img.sprite = null;
                            }
                        }
                    }
                }

                //Save the scene
                EditorSceneManager.SaveOpenScenes();
                RemoveDeletedScenesFromBuildSettings();
                WebGLBuildAndUpload.BuildUploadAndUpdate();

                //Refresh the images, so the image trackers that were set to not show in the web, show the image again
                ArTargetsLoader.LoadImagesAndType();
                //repaint the window
                Repaint();
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.white;

            if (EditorPrefs.GetString("LastAR_UrlId", "") != "")
            {
                if (GUILayout.Button(new GUIContent(" Abrir en web", webIcon),
                EditorStyles.miniButton, GUILayout.Width(150)))
                {
                    Application.OpenURL("https://my.expiria3d.com/" + EditorPrefs.GetString("LastAR_UrlId"));
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < categories.Count; i++)
            {
                GUILine(1);
                EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.toolbar)
                {
                    fixedHeight = 42,
                });

                Rect btnRect = EditorGUILayout.BeginHorizontal(btnStyle);
                {
                    string icon = !IsFoldout(categories[i].name) ? "d_forward@2x" : "d_icon dropdown@2x";
                    GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent(icon)), GUILayout.Width(30), GUILayout.Height(30));
                    //draw icon
                    GUILayout.Label(GetIcon(categories[i].iconName, false), GUILayout.Width(30), GUILayout.Height(30));

                    //flexible vertical
                    EditorGUILayout.BeginVertical(GUILayout.Height(30));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(categories[i].name, styleTxt);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                {
                    SetFoldout(categories[i].name, !IsFoldout(categories[i].name));
                    Repaint();
                }

                if (IsFoldout(categories[i].name))
                {

                    Rect r = EditorGUILayout.BeginVertical();

                    using (new GUILayout.VerticalScope(new GUIStyle() { margin = new RectOffset(10, 10, 10, 10) }))
                    {
                        if (i == 0)
                        {
                            DrawGeneralSettings(arManager);
                        }
                        else if (i == categories.Count - 1)
                        {
                            DrawWebResources();
                        }
                        else
                        {

                            float maxBoxSize = 100;
                            float spacing = 5f;
                            int cols = Mathf.FloorToInt((windowWidth + spacing - 35) / (maxBoxSize + spacing));
                            if (cols < 1) cols = 1;

                            float size = (windowWidth - (cols - 1) * spacing) / cols;
                            if (size > maxBoxSize) size = maxBoxSize;

                            bool horizontalStarted = false;
                            int numComponents = 0;

                            for (int j = 0; j < categories[i].items.Count; j++)
                            {
                                if (numComponents % cols == 0)
                                {
                                    // Close the previous horizontal group if one was started
                                    if (horizontalStarted)
                                    {
                                        EditorGUILayout.EndHorizontal();
                                        horizontalStarted = false;
                                    }

                                    // Start a new horizontal group
                                    EditorGUILayout.BeginHorizontal();
                                    horizontalStarted = true;
                                }

                                GUILayout.Space(spacing);
                                DrawComponentBox(i, j, size);

                                numComponents++;
                            }

                            // Close any remaining horizontal group
                            if (horizontalStarted)
                            {
                                EditorGUILayout.EndHorizontal();
                                horizontalStarted = false;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

                    // draw yellow line at left of the rect
                    EditorGUI.DrawRect(new Rect(r.x - 10, r.y - 10, 5, r.height + 21), new Color(1, 0.92f, 0.016f, 1));

                }
            }
        }

        void DrawWebResources()
        {
            GUIStyle styleTxt = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 16, wordWrap = true };

            EditorGUILayout.BeginVertical();

            string currentResourceType = "";

            for (int i = 0; i < resources.Count; i++)
            {
                if (currentResourceType != resources[i].Item4)
                {
                    if (i > 0)
                        GUILayout.Space(10);

                    currentResourceType = resources[i].Item4;

                    EditorGUILayout.LabelField(currentResourceType, new GUIStyle(styleTxt)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 20,
                        fontStyle = FontStyle.Bold,
                        wordWrap = true
                    });

                    //if its Modelos 3D, draw button to open pandora window
                    if (currentResourceType == "Modelos 3D")
                    {
                        //Draw the title of the type of resource
                        Rect re = EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.button) { padding = new RectOffset(10, 10, 7, 10) }
                        , GUILayout.Height(30));
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(GetIcon("Pandora_Icon.png", false, true), GUILayout.Width(30), GUILayout.Height(30));

                                EditorGUILayout.BeginVertical();
                                {
                                    GUILayout.Space(5);
                                    EditorGUILayout.LabelField("Abrir Pandora", styleTxt);
                                }
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        if (Event.current.type == EventType.MouseUp && re.Contains(Event.current.mousePosition) && Event.current.button == 0)
                        {
                            PandoraEditor.OpenWindow();
                            EditorPrefs.SetInt("tabSelectedInPandora", 1);
                        }
                    }
                }

                //Draw the title of the type of resource
                Rect r = EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.button) { padding = new RectOffset(10, 10, 7, 10) }
                , GUILayout.Height(30));
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(GetIcon(resources[i].Item3, false), GUILayout.Width(30), GUILayout.Height(30));

                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(5);
                            EditorGUILayout.LabelField(resources[i].Item2, styleTxt);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                if (Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition) && Event.current.button == 0)
                {
                    Application.OpenURL(resources[i].Item1);
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawGeneralSettings(ARManager arManager)
        {
            GUIStyle styleTxt = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 15, wordWrap = true };

            var loadingState = ArTargetsLoader.LoadingState;
            arManager.uGameProjectId = User.CurrentSessionInfo.project_id;

            if (string.IsNullOrEmpty(arManager.uGameProjectId))
            {
                ArActionsEditorTools.WarningWithBigFont("No se ha iniciado sesión en Expiria3D. Por favor, inicie sesión para continuar.");

                UGSServiceManager.UserValidation.ValidateUser(true);

                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.BeginVertical("groupbox");
            EditorGUILayout.BeginHorizontal();
            Texture icon_proj = GetIcon("ugame.png", false);

            EditorGUILayout.BeginVertical(GUILayout.Height(40), GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.Label(icon_proj, GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Proyecto", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 13, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) }, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(User.CurrentSessionInfo.project.name, styleTxt);
            EditorGUILayout.EndVertical();


            Texture changeIcon = EditorGUIUtility.IconContent("Import-Available").image;
            Texture openIcon = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small").image;
            Texture refreshIcon = EditorGUIUtility.IconContent("d_Refresh").image;
            Texture rotateIcon = EditorGUIUtility.IconContent("d_RotateTool").image;

            EditorGUILayout.BeginVertical(GUILayout.Width(100));
            //button to change the project
            if (GUILayout.Button(new GUIContent(" Salir", changeIcon), EditorStyles.miniButton, GUILayout.Width(100)))
            {
                User.Logout();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }
            if (GUILayout.Button(new GUIContent(" Abrir", openIcon), EditorStyles.miniButton, GUILayout.Width(100)))
            {
                Application.OpenURL("https://expiria3d.com/user/project?team_id=" + User.CurrentSessionInfo.team_id + "&project_id=" + User.CurrentSessionInfo.project_id + "&tab=web");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            if (GUILayout.Button(new GUIContent(" Refrescar", refreshIcon), GUILayout.Width(100)))
            {
                ArTargetsLoader.LoadImagesAndType();
                //repaint the window
                Repaint();
            }

            if (ARManager.Instance.arProjectType != ARManager.ARProjectType.None)
            {
                EditorGUILayout.BeginHorizontal("groupbox");

                string arProjectType = ARManager.Instance.arProjectType == ARManager.ARProjectType.Image ? "Imagen" : "Mundo";

                EditorGUILayout.LabelField("Tipo de seguimiento: " + arProjectType, styleTxt);

                if (GUILayout.Button(new GUIContent(" Cambiar", rotateIcon), EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    Application.OpenURL("https://expiria3d.com/user/project?team_id=" + User.CurrentSessionInfo.team_id + "&project_id=" + User.CurrentSessionInfo.project_id + "&tab=web");
                }

                EditorGUILayout.EndHorizontal();

                WorldTrackerObject wTrackerObj = FindObjectOfType<WorldTrackerObject>(true);
                if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                {
                    if (wTrackerObj != null)
                    {
                        ArActionsEditorTools.ErrorWithBigFont("Error crítico: Se ha detectado que esta escena tiene una configuración de Realidad Aumentada con 'Seguimiento de Mundo' pero seleccionaste 'Seguimiento de Imagen'. ¿Desea crear una nueva escena para seguimiento de Imagen?");
                        CreateImgScene();
                    }

                    EditorGUILayout.BeginVertical("groupbox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        Texture icon_ar = GetIcon("ar.png", false);
                        GUILayout.Label(icon_ar, GUILayout.Width(25), GUILayout.Height(25));
                        EditorGUILayout.LabelField("Imágenes de Seguimiento", styleTxt);
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        if (loadingState == ArTargetsLoader.ArTargetsLoaderState.None)
                        {
                            ArTargetsLoader.LoadImagesAndType();

                            //This is needed to avoid error on enter and exit play mode. Some related with control of GUI
                            EditorGUILayout.LabelField("Cargando...", GUILayout.Width(80));
                            Repaint();
                        }
                        else if (loadingState == ArTargetsLoader.ArTargetsLoaderState.SuccessWithNoImages)
                        {
                            ArActionsEditorTools.WarningWithBigFont("No se encontraron imágenes de seguimiento.\r\nPor favor, suba imágenes de seguimiento en su proyecto de Expiria3D, luego, haga clic en el botón 'Refrescar'.");

                            EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
                            Texture uploadIcon = EditorGUIUtility.IconContent("d_RawImage Icon").image;
                            if (GUILayout.Button(new GUIContent(" Subir Imagenes", uploadIcon)
                            , ArActionsEditorTools.BigButtonStyle, GUILayout.Height(38)))
                            {
                                Application.OpenURL("https://expiria3d.com/user/project?team_id=" + User.CurrentSessionInfo.team_id + "&project_id=" + User.CurrentSessionInfo.project_id + "&tab=ar");
                            }
                            //Refresh button
                            if (GUILayout.Button(new GUIContent(" Refrescar", refreshIcon), ArActionsEditorTools.BigButtonStyle, GUILayout.Height(38)))
                            {
                                ArTargetsLoader.LoadImagesAndType();
                                //repaint the window
                                Repaint();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (loadingState == ArTargetsLoader.ArTargetsLoaderState.Error)
                        {
                            ArActionsEditorTools.WarningWithBigFont("Ha ocurrido un error al cargar las imágenes de seguimiento. Por favor, intente nuevamente pulsando el botón 'Refrescar'.");
                        }
                        else if (loadingState == ArTargetsLoader.ArTargetsLoaderState.Loading)
                        {
                            EditorGUILayout.LabelField("Cargando...", GUILayout.Width(80));
                            Repaint();
                        }
                        else if (loadingState == ArTargetsLoader.ArTargetsLoaderState.Success)
                        {
                            //if the image trackers are less than the loaded sprites, create new image trackers

                            //clear the empty image trackers
                            arManager.imageTrackers.RemoveAll(x => x == null);
                            //clear the empty sprites
                            ArTargetsLoader.loadedSprites.RemoveAll(x => x == null);

                            if (ArTargetsLoader.loadedSprites.Count == 0)
                            {
                                ArTargetsLoader.LoadImagesAndType();
                                //This is needed to avoid error on enter and exit play mode. Some related with control of GUI
                                EditorGUILayout.LabelField("Cargando...", GUILayout.Width(80));
                                Repaint();
                            }
                            else
                            {
                                if (arManager.imageTrackers.Count < ArTargetsLoader.loadedSprites.Count)
                                {
                                    for (int i = arManager.imageTrackers.Count; i < ArTargetsLoader.loadedSprites.Count; i++)
                                    {
                                        GameObject go = new GameObject("Imagen de Seguimiento " + (i + 1));
                                        ImageTracker imageTracker = go.AddComponent<ImageTracker>();
                                        go.hideFlags = HideFlags.None;
                                        imageTracker.CreatePreview();
                                        imageTracker.targetIndex = i;
                                        arManager.imageTrackers.Add(imageTracker);
                                        imageTracker.hideFlags = HideFlags.HideInInspector;

                                        EditorUtility.SetDirty(imageTracker);
                                    }

                                    Repaint();
                                    EditorUtility.SetDirty(arManager);
                                }
                                else if (arManager.imageTrackers.Count > ArTargetsLoader.loadedSprites.Count)
                                {
                                    Repaint();
                                    ArActionsEditorTools.WarningWithBigFont("El número de imágenes de seguimiento en la escena es mayor que el número de imágenes cargadas. Por favor, elimine las imágenes de seguimiento adicionales. Debes tener " + ArTargetsLoader.loadedSprites.Count + " imágenes de seguimiento.");
                                }

                                for (int i = 0; i < arManager.imageTrackers.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();

                                    bool spritesInsideRange = i < ArTargetsLoader.loadedSprites.Count;
                                    bool trackersInsideRange = i < arManager.imageTrackers.Count;

                                    if (spritesInsideRange && trackersInsideRange && ArTargetsLoader.loadedSprites[i] != null && arManager.imageTrackers[i] != null)
                                    {
                                        Transform preview = arManager.imageTrackers[i].transform.Find("_TargetTrackingImagePreview");

                                        if (preview == null)
                                        {
                                            arManager.imageTrackers[i].CreatePreview();
                                        }

                                        Image img = arManager.imageTrackers[i].transform.Find("_TargetTrackingImagePreview").GetComponent<Image>();

                                        img.sprite = ArTargetsLoader.loadedSprites[i];

                                        Rect rImg = EditorGUILayout.BeginHorizontal();
                                        GUILayout.Label(ArTargetsLoader.loadedSprites[i].texture, GUILayout.Width(80), GUILayout.Height(80));
                                        EditorGUILayout.EndHorizontal();

                                        if (Event.current.type == EventType.MouseDown && rImg.Contains(Event.current.mousePosition) && Event.current.button == 0)
                                        {
                                            SceneView.lastActiveSceneView.FrameSelected();
                                            Selection.activeGameObject = arManager.imageTrackers[i].gameObject;
                                            EditorGUIUtility.PingObject(arManager.imageTrackers[i]);
                                            SceneView.lastActiveSceneView.FrameSelected();
                                            SceneView.lastActiveSceneView.FrameSelected(); //this is to focus the object in the scene view
                                        }
                                    }
                                    else
                                    {
                                        //Draw a button 80x80 with the text "ELIMINAR"
                                        if (GUILayout.Button("ELIMINAR", GUILayout.Width(80), GUILayout.Height(80)))
                                        {
                                            DestroyImmediate(arManager.imageTrackers[i].gameObject);
                                            arManager.imageTrackers.RemoveAt(i);
                                            EditorUtility.SetDirty(arManager);
                                            // End the horizontal group before exiting
                                            EditorGUILayout.EndHorizontal();
                                            GUIUtility.ExitGUI();
                                        }
                                    }

                                    EditorGUILayout.BeginVertical();

                                    Rect rTitle = EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Imagen de Seguimiento " + (i + 1), EditorStyles.boldLabel);
                                    EditorGUILayout.EndHorizontal();

                                    if (Event.current.type == EventType.MouseDown && rTitle.Contains(Event.current.mousePosition) && Event.current.button == 0)
                                    {
                                        SceneView.lastActiveSceneView.FrameSelected();
                                        Selection.activeGameObject = arManager.imageTrackers[i].gameObject;
                                        EditorGUIUtility.PingObject(arManager.imageTrackers[i]);
                                        SceneView.lastActiveSceneView.FrameSelected();
                                        SceneView.lastActiveSceneView.FrameSelected(); //this is to focus the object in the scene view
                                    }

                                    EditorGUIUtility.labelWidth = 80;

                                    EditorGUI.BeginChangeCheck();

                                    arManager.imageTrackers[i].orientation = (ImageTracker.Orientation)EditorGUILayout.EnumPopup("Orientación", arManager.imageTrackers[i].orientation);

                                    arManager.imageTrackers[i].showTrackingImageInWeb = EditorGUILayout.ToggleLeft("Mostrar Imagen en Versión Web", arManager.imageTrackers[i].showTrackingImageInWeb);

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        EditorUtility.SetDirty(arManager.imageTrackers[i]);
                                    }
                                    EditorGUIUtility.labelWidth = 0;

                                    EditorGUILayout.EndVertical();

                                    if (arManager.imageTrackers[i] != null) //this was added because if we delete the object, the object is null, and we can't access to the targetIndex
                                    {
                                        //If the targetIndex is different of i, change it
                                        if (arManager.imageTrackers[i].targetIndex != i)
                                        {
                                            arManager.imageTrackers[i].targetIndex = i;
                                            EditorUtility.SetDirty(arManager.imageTrackers[i]);
                                        }

                                        //if the name is different of "Imagen de Seguimiento" + i, change it
                                        if (arManager.imageTrackers[i].name != "Imagen de Seguimiento " + (i + 1))
                                        {
                                            arManager.imageTrackers[i].name = "Imagen de Seguimiento " + (i + 1);
                                            EditorUtility.SetDirty(arManager.imageTrackers[i]);
                                        }
                                    }

                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }

                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    if (wTrackerObj == null)
                    {
                        ArActionsEditorTools.ErrorWithBigFont("Error crítico: Seleccionaste 'Seguimiento de Mundo', pero esta escena no está configurada para soportar esta opción. ¿Desea crear una nueva escena para seguimiento de Mundo?");
                        CreateWorldScene();
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginVertical("groupbox");
                ArActionsEditorTools.WarningWithBigFont("No se ha activado la opción de Realidad Aumentada en el proyecto de Expiria3D. Por favor, active la opción en el proyecto de Expiria3D para continuar.");

                //Open the project in the browser
                if (GUILayout.Button(new GUIContent(" Abrir", openIcon), ArActionsEditorTools.BigButtonStyle))
                {
                    Application.OpenURL("https://expiria3d.com/user/project?team_id=" + User.CurrentSessionInfo.team_id + "&project_id=" + User.CurrentSessionInfo.project_id + "&tab=ar");
                }

                //Refresh
                if (GUILayout.Button(new GUIContent(" Refrescar", refreshIcon), ArActionsEditorTools.BigButtonStyle))
                {
                    ArTargetsLoader.LoadImagesAndType();
                    //repaint the window
                    Repaint();
                }

                EditorGUILayout.EndVertical();

            }


            EditorGUILayout.BeginVertical("groupbox");

            EditorGUILayout.BeginHorizontal();
            Texture icon = GetIcon("ar_light.png", false);
            GUILayout.Label(icon, GUILayout.Width(25), GUILayout.Height(25));
            EditorGUILayout.LabelField("Configuración de luz", styleTxt);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            //find Light in the scene, and HideInHierarchy
            Light light = FindFirstObjectByType<Light>();

            if (light != null)
            {
                if (AdvancedMode)
                {
                    light.gameObject.hideFlags = HideFlags.None;
                }
                else
                {
                    light.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            else
            {
                GameObject lightObj = new GameObject("Directional Light");
                lightObj.AddComponent<Light>();
                lightObj.GetComponent<Light>().type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
                lightObj.transform.position = new Vector3(0, 3, 0);
                lightObj.hideFlags = HideFlags.HideInHierarchy;

                light = lightObj.GetComponent<Light>();
                //set soft shadows
                light.shadows = LightShadows.Soft;

                //mark scene as dirty
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (light != null)
            {
                EditorGUI.BeginChangeCheck();
                //draw the field to define the color, intensity and rotation of the light
                light.color = EditorGUILayout.ColorField("Color de la Luz", light.color);
                light.intensity = EditorGUILayout.FloatField("Intensidad de la Luz", light.intensity);

                GUILayout.Space(10);
                //Button to move editor camera to light (focus it), select the light and change to rotation tool
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(
                    new GUIContent(" Restaurar Valores", refreshIcon)
                    , EditorStyles.miniButton, GUILayout.Width(140)))
                {
                    light.color = Color.white;
                    light.intensity = 1;
                    light.transform.rotation = Quaternion.Euler(50, -30, 0);
                    light.transform.position = new Vector3(0, 1, 0);
                }

                EditorGUILayout.Space();

                if (GUILayout.Button(
                    new GUIContent(" Cambiar Dirección", rotateIcon), EditorStyles.miniButton, GUILayout.Width(140)))
                {
                    SceneView.lastActiveSceneView.FrameSelected();
                    Selection.activeGameObject = light.gameObject;
                    UnityEditor.Tools.current = Tool.Rotate;
                    SceneView.lastActiveSceneView.FrameSelected();
                    SceneView.lastActiveSceneView.FrameSelected(); //this is to focus the object in the scene view
                }

                EditorGUILayout.EndHorizontal();

                Undo.RecordObject(light, "Change Light Settings");
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(light);
                }
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            AdvancedMode = EditorGUILayout.ToggleLeft("Modo Avanzado", AdvancedMode);
            EditorGUILayout.EndHorizontal();

            if (arManager != null) //This is to avoid an error when exporting. I do not know why, but the arManager is null at this point when finishing the export
            {
                if (AdvancedMode)
                {
                    arManager.gameObject.hideFlags = HideFlags.None;
                }
                else
                {
                    arManager.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }

                //find EventSystem in the scene, and HideInHierarchy
                UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

                if (eventSystem != null)
                {
                    if (AdvancedMode)
                    {
                        eventSystem.gameObject.hideFlags = HideFlags.None;
                    }
                    else
                    {

                        eventSystem.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
                else
                {
                    GameObject obj = new GameObject("EventSystem");
                    obj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    obj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

                    if (AdvancedMode)
                    {
                        obj.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        obj.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                ConfidenceDemo confidenceDemo = FindObjectOfType<ConfidenceDemo>();

                if (confidenceDemo != null)
                {
                    if (AdvancedMode)
                    {
                        confidenceDemo.gameObject.hideFlags = HideFlags.None;
                    }
                    else
                    {

                        confidenceDemo.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                ARCameraTarget aRCameraTarget = FindObjectOfType<ARCameraTarget>(true);

                if (aRCameraTarget != null)
                {
                    if (AdvancedMode)
                    {
                        aRCameraTarget.gameObject.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        aRCameraTarget.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                AudioListener audioListener = FindObjectOfType<AudioListener>(true);

                if (audioListener != null)
                {
                    if (AdvancedMode)
                    {
                        audioListener.gameObject.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        audioListener.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
            }
        }

        void DrawComponentBox(int catIndex, int itemIndex, float size)
        {
            Item item = categories[catIndex].items[itemIndex];
            string prefabPath = "";
            if (item.showPrefabThumbnail)
            {
                prefabPath = prefabSamplesPath + "/Model Prefab/" + item.prefabName + ".prefab";
            }
            else
            {
                prefabPath = prefabSamplesPath + "/" + item.prefabName + ".prefab";
            }


            if (!prefabs.ContainsKey(item.prefabName))
            {
                prefabs.Add(item.prefabName, AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            }

            if (prefabs[item.prefabName] == null)
            {
                Debug.LogError("Prefab not found: " + prefabPath);
                return;
            }

            itemStyle = new GUIStyle(EditorStyles.toolbarButton);
            itemStyle.fixedWidth = size;
            itemStyle.fixedHeight = size;

            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            GUILine(1);
            Rect componentBoxArea = EditorGUILayout.BeginVertical(itemStyle);
            {
                string name = item.name;

                if (item.showPrefabThumbnail)
                {
                    sampleIcon = GetIcon(item.name, true);
                }
                else
                    sampleIcon = GetIcon(item.iconName, false);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label(new GUIContent(sampleIcon, name), new GUIStyle("label") { padding = new RectOffset(5, 5, 5, 5), fixedHeight = size * 0.65f, alignment = TextAnchor.MiddleCenter }, GUILayout.Height(size * 0.65f), GUILayout.Width(size * 0.65f));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Label(new GUIContent(name, name), textStyle);
            }
            EditorGUILayout.EndVertical();

            GUI.color = Color.white;

            RegisterFieldForDrag(componentBoxArea, prefabs[item.prefabName]);

            GUILine(1, true);
            EditorGUILayout.EndVertical();
        }

        public void RegisterFieldForDrag(Rect fieldRect, UnityEngine.Object dragObject)
        {
            //ConditionContent ugameActionContentToDrag = UGS_ComponentsManager.instance.components[componentIndex].uGameActionContent;

            if (dragObject == null) return; // can't drag a null!

            Event e = Event.current;
            if (fieldRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDrag)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { dragObject };

                    DragAndDrop.StartDrag("drag");
                    Event.current.Use();
                }
            }
        }

        void GUILine(int i_height = 1, bool isBottom = false)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.width += 5;
            rect.x -= 4;

            if (isBottom)
                rect.y -= 2;
            else
                rect.y += 2;

            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 1));
        }

        void AddCurrentSceneToBuildSettings(bool showDialog = true)
        {
            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[original.Length + 1];

            // Copia los elementos originales, desplazándolos una posición hacia adelante
            Array.Copy(original, 0, newSettings, 1, original.Length);

            // Crea la nueva escena a agregar
            EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(EditorSceneManager.GetActiveScene().path, true);

            // Inserta la nueva escena en la primera posición
            newSettings[0] = sceneToAdd;

            // Actualiza las escenas de Build Settings
            EditorBuildSettings.scenes = newSettings;

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Escena agregada en el Build Settings"
                    , "Escena agregada en el Build Settings",
                    "Continuar");

                GUIUtility.ExitGUI();
            }
        }

        bool ThisSceneExistInBuildSettings()
        {
            //VERIFY EXISTENCE IN THE BUILD SETTINGS

            EditorBuildSettingsScene[] allScenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in allScenes)
            {
                if (scene.enabled && scene.path.Contains(EditorSceneManager.GetActiveScene().name))
                {
                    return true;
                }
            }

            //--------------------------------------

            return false;
        }

    }
}