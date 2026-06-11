namespace Expiria3DSpace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Networking;
    using System.Collections;
    using Expiria3DSpace.UGSService.Controller;

    public class PandoraEditor : EditorWindow
    {
        Pandora pandora;
        Vector2 scrollPos;
        Vector2 scrollPosBtns;
        Vector2 scrollPosMaterials;
        static Grid grid;

        static UnityEngine.Object objSelected;
        static UnityEngine.Object objInHandle;
        static float zPosToInstantiate;

        static Texture lookAtCameraTexture;

        float height = 0;
        float width = 0;

        float objectSizeMultiplier = 1;

        static bool objSelectedIsUI = false;

        bool showFavorites = false;
        bool showAnimatedInStock = false;
        string textToSearch = "";
        string textToSearchMat = "";

        static Texture textureBack = null;
        float _currMenuWidth = 180;

        int dropIndex = -1;
        List<GameObject> objsBeingDropped = new List<GameObject>();
        Pandora.PandoraObjectOfCategory objBeingDragged = null;

        bool removeElementsMode = false;

        bool hasEmptyObjects = false;

        Vector2 categoryInEdition = new Vector2(-1, -1);

        bool collapseMaterialMenu = true;
        string categoryMaterial = "Current Category";
        float _currMaterialMenuWidth = 155;

        bool mouseIsOverMaterialsToolbar = false;
        bool maximizeMaterials = false;

        int catDropIndex = -1;
        int subCatDropIndex = -1;

        int currentTab = 0;
        static bool pandoraAssetsLoaded = false;

        float categoryMenuWidth
        {
            get
            {
                return collapseMenu ? 0 : _currMenuWidth;
            }
            set
            {
                _currMenuWidth = value;
            }
        }

        float materialMenuWidth
        {
            get
            {
                if (collapseMaterialMenu)
                    return 0;
                else
                {
                    if (maximizeMaterials)
                    {
                        return this.position.width - categoryMenuWidth;
                    }
                    else
                    {
                        return _currMaterialMenuWidth;
                    }
                }
            }
            set
            {
                _currMaterialMenuWidth = value;
            }
        }

        bool menuIsBeingResized = false;
        bool materialMenuIsBeingResized = false;

        string[] materialDirectories = new string[0];
        string customMaterialsPath = "";

        bool collapseMenu = false;

        int totalPagesInStock = 0;
        string downloadedAssetID = "";
        string downloadedFromCatID = "";
        string downloadedFromSubCatID = "";


        [MenuItem("Expiria3D/Pandora")]
        public static void OpenWindow()
        {
            PandoraEditor window = EditorWindow.GetWindow<PandoraEditor>();

            // Loads an icon from an image stored at the specified path
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/Pandora_Icon.png");
            // Create the instance of GUIContent to assign to the window. Gives the title and the icon
            GUIContent titleContent = new GUIContent(" Pandora", icon);
            window.titleContent = titleContent;
        }

        static string SendPandoraBackendRequest(string dataParams)
        {
            WWWForm form = new WWWForm();
            form.AddField("data", "pandora_stock¦" + User.CurrentSessionInfo.jwt + "¦" + dataParams);

            UnityWebRequest www = UnityWebRequest.Post(User.ServiceURL, form);
            // Esto bloqueará la UI hasta que se complete la solicitud
            www.SendWebRequest();

            while (!www.isDone)
            {
            }

            string response = www.downloadHandler?.text;

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            if (www.responseCode == 200)
            {
                //return the response body
                return response;
            }
            if (response == "WRONG_JWT")
            {
                User.Logout();
                return null;
            }
            else
            {
                EditorUtility.DisplayDialog("Error with " + dataParams + " Code: " + www.responseCode, response, "Ok");
                return null;
            }
        }

        static void LoadCategoriesAndSubCategories()
        {
            string jsonForCategories = SendPandoraBackendRequest("load_categories");
            string jsonForSubCategories = SendPandoraBackendRequest("load_subcategories");
            string jsonForLikedAssets = SendPandoraBackendRequest("load_stars");

            if (!string.IsNullOrEmpty(jsonForCategories) && !string.IsNullOrEmpty(jsonForSubCategories) && !string.IsNullOrEmpty(jsonForLikedAssets))
            {
                Pandora.instance.cloudStockCategories = Expiria3DJsonHelper.FromJson<Pandora.CloudStockCategory>(jsonForCategories);
                Pandora.instance.cloudStockSubCategories = Expiria3DJsonHelper.FromJson<Pandora.CloudStockSubCategory>(jsonForSubCategories);
                Pandora.instance.likedAssetsInStock = Expiria3DJsonHelper.FromJson<Pandora.LikedAssetsInStock>(jsonForLikedAssets);

                if (Pandora.instance.cloudStockCategories.Length > 0)
                {
                    EditorPrefs.SetString("CloudCategorySelected", Pandora.instance.cloudStockCategories[0].id);
                    Pandora.CloudStockSubCategory subCat = Pandora.instance.cloudStockSubCategories.FirstOrDefault(x => x.parent_category_id == Pandora.instance.cloudStockCategories[0].id);

                    string scID;

                    if (subCat != null)
                        scID = subCat.id;
                    else
                        scID = "";

                    EditorPrefs.SetString("CloudSubcategorySelected", scID);

                    LoadAssetsFromCloud(Pandora.instance.cloudStockCategories[0].id, scID);
                }
            }
        }

        static void LoadAssetsFromCloud(string catID, string subCatID)
        {

            string jsonForAssets = SendPandoraBackendRequest("load_assets¦" + catID + "¦" + subCatID);

            if (!string.IsNullOrEmpty(jsonForAssets))
            {
                Pandora.instance.cloudAssetsInView = Expiria3DJsonHelper.FromJson<Pandora.CloudStockAsset>(jsonForAssets);
                pandoraAssetsLoaded = true;
            }

        }

        void OnEnable()
        {
            pandora = Pandora.instance;

            lookAtCameraTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/lookAtCameraIcon.png");

            SceneView.duringSceneGui += GridUpdate;
            EditorSceneManager.sceneOpened += NewSceneOpened;

            textureBack = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/bg.png");

            //OnFocus();

            if (EditorPrefs.GetInt("tabSelectedInPandora", 0) == 1)
            {
                LoadCategoriesAndSubCategories();
            }

            AssetDatabase.importPackageCompleted -= OnDownloadEnd;
            AssetDatabase.importPackageCompleted += OnDownloadEnd;
        }

        private void OnDestroy()
        {
            AssetDatabase.importPackageCompleted -= OnDownloadEnd;
        }

        void OnDownloadEnd(string packageName)
        {
            if (string.IsNullOrEmpty(downloadedAssetID) || string.IsNullOrEmpty(downloadedFromCatID) || string.IsNullOrEmpty(downloadedFromSubCatID))
            {
                return;
            }

            string catName = pandora.cloudStockCategories.FirstOrDefault(x => x.id == downloadedFromCatID)?.name_eng;
            if (string.IsNullOrEmpty(catName))
                catName = downloadedFromCatID;

            Pandora.CloudStockSubCategory subCategory = pandora.cloudStockSubCategories.FirstOrDefault(x => x.id == downloadedFromSubCatID);

            string subCatName = subCategory?.name_eng;
            if (string.IsNullOrEmpty(subCatName))
                subCatName = downloadedFromSubCatID;
            string finalPath = "Assets/Pandora Downloads/" + catName + "/" + subCatName + "/" + downloadedAssetID;

            if (AssetDatabase.IsValidFolder("Assets/Editor/" + downloadedAssetID))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Pandora Downloads"))
                {
                    AssetDatabase.CreateFolder("Assets", "Pandora Downloads");
                }

                if (!AssetDatabase.IsValidFolder("Assets/Pandora Downloads/" + catName))
                {
                    AssetDatabase.CreateFolder("Assets/Pandora Downloads", catName);
                }

                if (!AssetDatabase.IsValidFolder("Assets/Pandora Downloads/" + catName + "/" + subCatName))
                {
                    AssetDatabase.CreateFolder("Assets/Pandora Downloads/" + catName, subCatName);
                }

                Pandora.PandoraCategory pandoraDownloadsCateg = pandora.categories.FirstOrDefault(x => x.name == "Pandora Downloads");
                Pandora.PandoraSubcategory subCat = null;

                if (pandoraDownloadsCateg != null)
                    subCat = pandoraDownloadsCateg.subCategories.FirstOrDefault(x => x.name == catName + "/" + subCatName);

                if (pandoraDownloadsCateg == null)
                {
                    Pandora.PandoraCategory cat = new Pandora.PandoraCategory("Pandora Downloads");
                    cat.subCategories = new List<Pandora.PandoraSubcategory>() { new Pandora.PandoraSubcategory(catName + "/" + subCatName) };
                    subCat = cat.subCategories[0];
                    pandora.categories.Add(cat);
                    pandoraDownloadsCateg = pandora.categories.FirstOrDefault(x => x.name == "Pandora Downloads");
                }
                else
                {
                    if (subCat == null)
                    {
                        pandoraDownloadsCateg.subCategories.Add(new Pandora.PandoraSubcategory(catName + "/" + subCatName));
                        subCat = pandoraDownloadsCateg.subCategories.FirstOrDefault(x => x.name == catName + "/" + subCatName);
                    }
                }

                Pandora.CloudStockAsset asset = Pandora.instance.cloudAssetsInView.FirstOrDefault(x => x.id == downloadedAssetID);

                if (asset != null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Editor/" + downloadedAssetID });
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrEmpty(path))
                        {
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (go != null)
                            {
                                if (pandoraDownloadsCateg != null)
                                {
                                    if (!subCat.objects.Exists(x => x.obj == go))
                                    {
                                        subCat.objects.Add(new Pandora.PandoraObjectOfCategory(go));
                                    }
                                }
                            }
                        }
                    }
                }

                FileUtil.MoveFileOrDirectory("Assets/Editor/" + downloadedAssetID, finalPath);
                FileUtil.DeleteFileOrDirectory("Assets/Editor/" + downloadedAssetID + ".meta");
                FileUtil.DeleteFileOrDirectory("Assets/Editor/" + downloadedAssetID);

                if (pandoraDownloadsCateg != null && subCat != null)
                {
                    EditorPrefs.SetInt("CategorySelected", pandora.categories.IndexOf(pandoraDownloadsCateg));
                    EditorPrefs.SetInt("SubcategorySelected", pandoraDownloadsCateg.subCategories.IndexOf(subCat));
                    EditorPrefs.SetInt("tabSelectedInPandora", 0);
                }

                AssetDatabase.SaveAssets();
                textToSearch = "";
                showFavorites = false;
            }
            else
            {
                if (AssetDatabase.IsValidFolder(finalPath))
                {
                    Debug.Log("<color=yellow>Expiria3D: </color>You already have this asset in your project: " + downloadedAssetID);
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(finalPath, typeof(UnityEngine.Object)));
                }
                else
                    Debug.Log("<color=yellow>Expiria3D: </color>Error downloading asset: " + downloadedAssetID);
            }
        }

        //private void OnFocus()
        //{
        //    customMaterialsPath = Application.dataPath + "/Expiria3D/PandoraAssets/Builder/Materials/";
        //    materialDirectories = Directory.GetDirectories(customMaterialsPath, "*", SearchOption.AllDirectories);

        //    for (int i = 0; i < materialDirectories.Length; i++)
        //    {
        //        materialDirectories[i] = materialDirectories[i].Replace(customMaterialsPath, "").Replace("\\", "/");
        //    }

        //    Pandora.instance.cacheMaterialTextures = new Dictionary<string, Texture2D>();
        //}

        void NewSceneOpened(Scene scene, OpenSceneMode mode)
        {
            objSelected = null;
            if (objInHandle != null)
            {
                DestroyImmediate(objInHandle);
            }
            objInHandle = null;
            grid = null;
            ChangeToolToRect();
        }

        public static void ChangeToolToView()
        {
            UnityEditor.Tools.current = Tool.View;
        }

        public static void ChangeToolToRect()
        {
            UnityEditor.Tools.current = Tool.Rect;
        }

        public static Tool GetCurrentTool()
        {
            return UnityEditor.Tools.current;
        }

        void CreateGrid()
        {
            if (FindObjectOfType<Grid>() != null)
            {
                grid = FindObjectOfType<Grid>();
            }
            else
            {
                GameObject newGO = new GameObject();
                newGO.AddComponent<Grid>();
                newGO.transform.position = new Vector3(0, 0, 0);
                grid = newGO.GetComponent<Grid>();
                newGO.name = "GRID";
                newGO.gameObject.hideFlags = HideFlags.NotEditable;
                grid.hideFlags = HideFlags.None;
            }
        }

        public void OnDisable()
        {
            DestroyImmediate(objInHandle);
            SceneView.duringSceneGui -= GridUpdate;
            EditorSceneManager.sceneOpened -= NewSceneOpened;
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

        public bool RegisterFieldForDrag(Rect fieldRect, Pandora.PandoraObjectOfCategory obj)
        {
            if (obj.obj == null) return false; // can't drag a null!

            Event e = Event.current;
            if (fieldRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    objBeingDragged = obj;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { obj.obj };
                    DragAndDrop.StartDrag("drag");
                    Event.current.Use();
                }
                else if (e.type == EventType.MouseUp && e.button == 1)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Edit Prefab"), false, () =>
                    {
                        AssetDatabase.OpenAsset(obj.obj);
                    });

                    menu.AddItem(new GUIContent("View in project window"), false, () =>
                    {
                        EditorGUIUtility.PingObject(obj.obj);
                    });

                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        obj.obj = null;
                    });

                    menu.ShowAsContext();
                    e.Use();

                    objBeingDragged = null;
                    return false;
                }
                else if (e.type == EventType.MouseUp)
                {
                    objBeingDragged = null;
                    return true;
                }
            }

            return false;
        }

        void GridUpdate(SceneView sceneview)
        {
            Event _event = Event.current;
            Vector3 mousePos = new Vector3();

            try
            {
                Ray r = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector3(_event.mousePosition.x, -_event.mousePosition.y + SceneView.currentDrawingSceneView.camera.pixelHeight));
                mousePos = new Vector3(r.origin.x, r.origin.y);
            }
            catch
            {
                Debug.Log("<color=yellow>Expiria3D: </color>Oops!, try again.");
            }

            if (objSelected != null)
            {
                mousePos.z = zPosToInstantiate;
                if (objInHandle == null)
                {
                    objInHandle = Instantiate(objSelected);
                    objInHandle.hideFlags = HideFlags.HideAndDontSave;

                    if (objSelectedIsUI)
                    {
                        if (pandora.parent == null && FindObjectOfType<Canvas>() != null)
                        {
                            pandora.showParent = true;
                            pandora.parent = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
                        }

                        if (pandora.parent != null)
                        {
                            if ((GameObject)objInHandle != null)
                            {
                                ((GameObject)objInHandle).transform.SetParent(pandora.parent);
                                ((GameObject)objInHandle).transform.localScale = new Vector3(1, 1, 1);
                            }
                        }
                    }
                }
                else
                {
                    if (!objSelectedIsUI)
                    {
                        if ((GameObject)objInHandle != null)
                        {
                            ((GameObject)objInHandle).transform.localScale = ((GameObject)objSelected).transform.localScale * objectSizeMultiplier;
                        }
                    }
                }

                Vector3 aligned = new Vector3();
                if (EditorPrefs.GetBool("FitWithGrid", false))
                {
                    if (grid != null)
                        aligned = new Vector3(Mathf.Floor(mousePos.x / grid.cellSize.x) * grid.cellSize.x + grid.cellSize.x / 2.0f,
                                                      Mathf.Floor(mousePos.y / grid.cellSize.y) * grid.cellSize.y + grid.cellSize.y / 2.0f, zPosToInstantiate);

                }
                else
                {
                    aligned = mousePos;
                }

                if ((GameObject)objInHandle != null)
                {
                    ((GameObject)objInHandle).transform.position = aligned;
                }
            }

            if (_event.type == EventType.MouseDown && _event.button == 0)
            {
                if (objSelectedIsUI && pandora.parent == null)
                {
                    Debug.LogWarning("<color=yellow>Expiria3D: </color>Please assign a Canvas on the Pandora Editor to create the UI object.");
                }
                else
                {
                    GameObject obj;

                    if (objSelected != null)
                    {
                        if (GetCurrentTool() != Tool.Move)
                        {
                            ChangeToolToView();
                        }

                        //-----------------------
                        obj = (GameObject)PrefabUtility.InstantiatePrefab(objSelected);

                        if (!objSelectedIsUI)
                        {
                            obj.transform.localScale = ((GameObject)objSelected).transform.localScale * objectSizeMultiplier;
                            if (pandora.parent != null)
                                obj.transform.SetParent(pandora.parent);
                        }
                        else
                        {
                            obj.transform.SetParent(pandora.parent);
                            obj.transform.localScale = new Vector3(1, 1, 1);
                        }

                        //-----------------------

                        EditorGUIUtility.PingObject(obj);

                        Vector3 aligned;
                        if (EditorPrefs.GetBool("FitWithGrid", false))
                        {
                            aligned = new Vector3(Mathf.Floor(mousePos.x / grid.cellSize.x) * grid.cellSize.x + grid.cellSize.x / 2.0f,
                                                        Mathf.Floor(mousePos.y / grid.cellSize.y) * grid.cellSize.y + grid.cellSize.y / 2.0f, zPosToInstantiate);

                        }
                        else
                        {
                            aligned = new Vector3(mousePos.x, mousePos.y, zPosToInstantiate);
                        }

                        obj.transform.position = aligned;

                        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
                    }
                }
            }
        }

        void OnGUI()
        {
            if (textureBack == null)
                textureBack = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/bg.png");

            if (pandora == null) pandora = Pandora.instance;

            if (!UGSServiceManager.UserValidation.ValidateUser(false)) return;

            currentTab = EditorPrefs.GetInt("tabSelectedInPandora", 0);

            if (AssetPreview.IsLoadingAssetPreviews())
                Repaint();

            if (pandora.categories.Count == 0)
            {
                pandora.categories.Add(new Pandora.PandoraCategory("New category"));
            }
            else if (EditorPrefs.GetInt("CategorySelected", 0) > pandora.categories.Count - 1)
            {
                EditorPrefs.SetInt("CategorySelected", 0);
            }
            else
            {
                if (EditorPrefs.GetInt("SubcategorySelected", 0) != -1)
                {
                    if (EditorPrefs.GetInt("SubcategorySelected", 0) > pandora.categories[EditorPrefs.GetInt("CategorySelected", 0)].subCategories.Count - 1)
                    {
                        EditorPrefs.SetInt("SubcategorySelected", -1);
                    }
                }
            }

            if (hasEmptyObjects)
            {
                //Clean before init the GUI to avoid exceptions
                CleanEmptyObjects(EditorPrefs.GetInt("CategorySelected", 0), EditorPrefs.GetInt("SubcategorySelected", 0));
            }


            if (grid == null)
            {
                //if 2D perspective
                if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.in2DMode)
                {
                    //if grid enabled
                    if (EditorPrefs.GetBool("_ShowGrid", false))
                    {
                        CreateGrid();
                    }
                }
                else //Its in 3D perspective
                {
                    if (objInHandle != null) //has an object selected, which is not allowed
                    {
                        Deselect();
                    }
                }
            }
            else
            {
                if (SceneView.lastActiveSceneView != null && !SceneView.lastActiveSceneView.in2DMode)
                {
                    if (objInHandle != null)
                    {
                        Deselect();
                    }

                    if (grid != null)
                    {
                        EditorWindow.focusedWindow.ShowNotification(new GUIContent("No se permite la cuadrícula en vista 3D. Cuadrícula desactivada."));

                        DestroyImmediate(grid.gameObject);
                    }

                    EditorPrefs.SetBool("_ShowGrid", false);

                }
            }

            //-------------------------------------------------------------------------------------- /
            //Variables for scroll
            width = this.position.width;
            height = this.position.height - 4;

            if (textureBack != null)
                GUI.DrawTexture(new Rect(categoryMenuWidth + materialMenuWidth, 0, this.width - categoryMenuWidth - materialMenuWidth, Screen.height), textureBack, ScaleMode.StretchToFill, false, 0, new Color(1, 1, 1, 1f), 0, 0);

            EditorGUILayout.BeginHorizontal();
            if (!collapseMenu)
                Menu();

            if (!collapseMaterialMenu)
            {
                DrawMaterialMenu();
            }

            LevelEditorTab();
            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(pandora);
            Undo.RecordObject(pandora, "Undo pandora");

            if (grid != null)
            {
                if (EditorUtility.IsDirty(grid)) EditorUtility.SetDirty(grid);
                Undo.RecordObject(grid, "Undo grid");
            }
        }

        void DrawMaterialMenu()
        {
            if (!maximizeMaterials)
            {
                VerticalResizableArea(ref _currMaterialMenuWidth, ref materialMenuIsBeingResized, this, categoryMenuWidth);
            }
            EditorGUILayout.BeginVertical();
            {
                scrollPosMaterials = GUILayout.BeginScrollView(scrollPosMaterials, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(materialMenuWidth), GUILayout.Height(this.position.height - 15));
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(materialMenuWidth - 14));
                    {
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Folder Icon").image, "Custom Folders"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {

                        }

                        string name = "";
                        if (categoryMaterial.StartsWith("Custom Folders"))
                        {
                            //Get the name of the category, based on the last folder of the split
                            string[] split = categoryMaterial.Replace("Custom Folders/", "").Split('/');
                            if (split.Length == 0 || (split.Length > 0 && split[0] == ""))
                            {
                                categoryMaterial = materialDirectories[0];
                                split = categoryMaterial.Split('/');
                            }

                            name = split.Length == 0 ? "" : split[split.Length - 1];
                        }
                        else if (categoryMaterial.StartsWith("My Collections"))
                        {
                            string[] split = categoryMaterial.Split('/');
                            if (split.Length == 2)
                            {
                                name = pandora.categories[Convert.ToInt32(split[1])].name;
                            }
                            else if (split.Length == 3)
                            {
                                name = pandora.categories[Convert.ToInt32(split[1])].subCategories[Convert.ToInt32(split[2])].name;
                            }
                        }
                        else
                        {
                            name = categoryMaterial;
                        }
                        //--------------------------------------------------------------------

                        if (GUILayout.Button(name, new GUIStyle(EditorStyles.toolbarPopup) { fontSize = 11 }))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddItem(new GUIContent("Current Category"), false, () =>
                            {
                                categoryMaterial = "Current Category";
                            });

                            menu.AddItem(new GUIContent("Created Materials"), false, () =>
                            {
                                categoryMaterial = "Created Materials";
                            });

                            menu.AddItem(new GUIContent("Custom Folders/[Customize Folders]"), false, () =>
                            {

                            });

                            for (int i = 0; i < materialDirectories.Length; i++)
                            {
                                int cat = i;
                                int numMats = Directory.GetFiles(customMaterialsPath + materialDirectories[cat], "*.mat", SearchOption.TopDirectoryOnly).Length;
                                if (materialDirectories[cat].Contains("/") || numMats > 0)
                                {
                                    menu.AddItem(new GUIContent("Custom Folders/" + materialDirectories[cat] + " [" + numMats + "]"), categoryMaterial == materialDirectories[cat], () =>
                                    {
                                        categoryMaterial = "Custom Folders/" + materialDirectories[cat];
                                        //EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Expiria3D/PandoraAssets/Materials/" + categoryMaterial, typeof(UnityEditor.DefaultAsset)));
                                    });
                                }
                            }

                            menu.AddSeparator("");

                            for (int i = 0; i < pandora.categories.Count; i++)
                            {
                                int cat = i;
                                menu.AddItem(new GUIContent("My Collections/" + pandora.categories[cat].name), false, () =>
                                {
                                    categoryMaterial = "My Collections/" + cat;
                                });

                                if (pandora.categories[i].subCategories.Count > 0)
                                {
                                    for (int j = 0; j < pandora.categories[cat].subCategories.Count; j++)
                                    {
                                        int subCat = j;
                                        menu.AddItem(new GUIContent("My Collections/" + pandora.categories[cat].name + "/" + pandora.categories[cat].subCategories[subCat].name), false, () =>
                                        {
                                            categoryMaterial = "My Collections/" + cat + "/" + subCat;
                                        });
                                    }
                                }
                            }

                            menu.ShowAsContext();
                        }

                        if (materialMenuWidth > 150)
                        {
                            GUILayout.Space(3);
                            textToSearchMat = EditorGUILayout.TextField(textToSearchMat, ToolbarSearchBar.SearchBarStyle);

                            if (GUILayout.Button("", ToolbarSearchBar.CancelBtnStyle))
                            {
                                // Remove focus if cleared
                                textToSearchMat = "";
                                GUI.FocusControl(null);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    //---------------------------------------------------------------
                    //DRAW MATERIALS
                    EditorGUILayout.BeginVertical();
                    int numMaterials = DrawMaterials();
                    EditorGUILayout.EndVertical();
                    //---------------------------------------------------------------

                    if (categoryMaterial == "Created Materials" || categoryMaterial.StartsWith("Custom Folders"))
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        {
                            GUI.backgroundColor = Color.white / 1.2f;
                            if (GUILayout.Button(new GUIContent("Create", EditorGUIUtility.IconContent("P4_AddedRemote").image), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft }))
                            {
                                GUI.FocusControl(null);
                                Material mat = new Material(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"));
                                mat.color = Color.white;
                                string path = "Assets/Expiria3D/PandoraAssets/CreatedMaterials/Material-" + numMaterials + ".mat";
                                ProjectWindowUtil.CreateAsset(mat, path);
                                categoryMaterial = "Created Materials";
                            }

                            GUI.backgroundColor = Color.white;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
                Rect r = EditorGUILayout.BeginHorizontal(new GUIStyle(EditorStyles.toolbar) { fixedWidth = materialMenuWidth });
                {
                    if (materialMenuWidth > 72)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(EditorGUIUtility.IconContent("back").image, new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleCenter, padding = new RectOffset(0, 0, 0, 4) }, GUILayout.Width(20)))
                            {

                            }
                            EditorGUILayout.LabelField("1/1", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter }, GUILayout.Width(25));

                            if (GUILayout.Button(EditorGUIUtility.IconContent("forward").image, new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleCenter, padding = new RectOffset(0, 0, 0, 4) }, GUILayout.Width(20)))
                            {

                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Width(70));
                    {
                        if (materialMenuWidth > 147)
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Height(10));
                            pandora.materialBoxSize = (int)GUI.HorizontalSlider(new Rect(rect.position, new Vector2(rect.size.x - 25, rect.size.y)), pandora.materialBoxSize, 25, 135);
                            EditorGUILayout.EndVertical();
                        }

                        if (materialMenuWidth > 90)
                        {
                            GUI.backgroundColor = maximizeMaterials ? Color.green : Color.white;
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("winbtn_win_max").image, "Full Screen"), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.UpperCenter, padding = new RectOffset(0, 0, 0, 1) }, GUILayout.Width(20)))
                            {
                                maximizeMaterials = !maximizeMaterials;
                            }

                            GUI.backgroundColor = Color.white;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();

                mouseIsOverMaterialsToolbar = (r != new Rect(0, 0, 0, 0)) ? r.Contains(Event.current.mousePosition) : mouseIsOverMaterialsToolbar;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the quantity of materials at the folder</returns>
        int DrawMaterials()
        {
            if (!Directory.Exists(customMaterialsPath + categoryMaterial.Replace("Custom Folders/", ""))
                && categoryMaterial.StartsWith("Custom Folders/"))
            {
                categoryMaterial = "Current Category";
                return 0;
            }

            List<string> materialPaths = new List<string>();

            if (categoryMaterial.StartsWith("Custom Folders"))
            {
                materialPaths = Directory.GetFiles(customMaterialsPath + categoryMaterial.Replace("Custom Folders/", ""), "*.mat", SearchOption.TopDirectoryOnly).ToList();
            }
            else if (categoryMaterial == "Created Materials")
            {
                materialPaths = Directory.GetFiles(Application.dataPath + "/Expiria3D/PandoraAssets/CreatedMaterials", "*.mat", SearchOption.AllDirectories).ToList();
            }
            else if (categoryMaterial == "Current Category")
            {
                int cat = EditorPrefs.GetInt("CategorySelected", 0);
                int subCat = EditorPrefs.GetInt("SubcategorySelected", 0);

                GetMaterialPathsFromCat(cat, subCat, ref materialPaths);
            }
            else if (categoryMaterial.StartsWith("My Collections"))
            {
                string[] split = categoryMaterial.Split('/');

                if (split.Length == 2)
                {
                    GetMaterialPathsFromCat(Convert.ToInt32(split[1]), -1, ref materialPaths);
                }
                else if (split.Length == 3)
                {
                    GetMaterialPathsFromCat(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), ref materialPaths);
                }
            }

            bool horizontalEnded = true;
            //int cols = (int)((materialMenuWidth-14) / pandora.materialBoxSize); //width is the fixedWidth of every element.

            float windowWidth = materialMenuWidth - 14; //14 is the scrollbar, 18 is other UI elements
            windowWidth -= (int)(windowWidth / pandora.materialBoxSize) * 2; //2 is an additional space between the elements.
            int cols = (int)(windowWidth / pandora.materialBoxSize);
            float size = windowWidth / cols; //the size of the box
            if (size > 138)
                size = 138;

            if (cols == 0) cols = 1;

            int numObjs = 0;
            for (int i = 0; i < materialPaths.Count; i++)
            {
                string assetPath = "";

                if (categoryMaterial == "Created Materials" || categoryMaterial.StartsWith("Custom Folders"))
                {
                    assetPath = "Assets" + materialPaths[i].Replace(Application.dataPath, "").Replace('\\', '/').Replace("Custom Folders/", "");
                }
                else
                    assetPath = materialPaths[i];

                Material material = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material));
                if (material == null)
                    Debug.Log("<color=yellow>Expiria3D: </color>" + assetPath);
                if (material.name.ToLower().Contains(textToSearchMat.ToLower()))
                {
                    if (numObjs != 0 && numObjs % cols == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        horizontalEnded = true;
                    }

                    if (numObjs % cols == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        horizontalEnded = false;
                    }

                    numObjs++;
                    if (!pandora.cacheMaterialTextures.ContainsKey(assetPath))
                    {
                        pandora.cacheMaterialTextures.Add(assetPath, null);
                    }

                    if (pandora.cacheMaterialTextures[assetPath] == null)
                    {
                        if (!AssetPreview.IsLoadingAssetPreviews())
                        {
                            pandora.cacheMaterialTextures[assetPath] = AssetPreview.GetAssetPreview(material); // GetPreviewTextureWithAlpha(material);
                        }
                    }

                    //-------------------------------------------------------------------
                    EditorGUILayout.BeginHorizontal(new GUIStyle(EditorStyles.toolbar) { fixedHeight = size + 1 }, GUILayout.Width(size));
                    RectOffset rectZero = new RectOffset(0, 0, 0, 0);
                    Rect rect = EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.toolbarButton) { fixedHeight = size, fixedWidth = size + 1, padding = rectZero, margin = rectZero });
                    {
                        Event e = Event.current;

                        if (!mouseIsOverMaterialsToolbar) //Avoid the drag on elements when mouse is over toolbar
                            RegisterFieldForDrag(rect, material);

                        if (rect.Contains(e.mousePosition) && e.type == EventType.MouseUp)
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddItem(new GUIContent("Edit"), false, () =>
                            {
                                Selection.activeObject = material;
                                EditorGUIUtility.PingObject(material);
                            });

                            menu.AddItem(new GUIContent("Duplicate"), false, () =>
                            {
                                ProjectWindowUtil.CreateAsset(new Material(material), "Assets/Expiria3D/PandoraAssets/CreatedMaterials/" + material.name.Replace(".mat", "") + " Copy.mat");
                                categoryMaterial = "Created Materials";
                            });

                            menu.ShowAsContext();
                        }

                        string name = material.name;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent(pandora.cacheMaterialTextures[assetPath] == null ? AssetPreview.GetMiniThumbnail(material) : pandora.cacheMaterialTextures[assetPath], name),
                            new GUIStyle("label") { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(2, 5, 2, 2), margin = rectZero }, GUILayout.Height(size), GUILayout.Width(size));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    //--------------------------------------------------------------------------------
                }
            }

            if (!horizontalEnded)
            {
                EditorGUILayout.EndHorizontal();
            }

            return materialPaths.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="subCat"></param>
        /// <param name="materialPaths"></param>
        /// <param name="isForCollectionsOrCurrent">0 = collections, 1 = current</param>
        void GetMaterialPathsFromCat(int cat, int subCat, ref List<string> materialPaths)
        {
            List<Pandora.PandoraObjectOfCategory> objects;
            if (subCat == -1)
                objects = pandora.categories[cat].objects;
            else
                objects = pandora.categories[cat].subCategories[subCat].objects;

            GameObject[] gameObjects = objects.Where(x => x.type == Pandora.PandoraObjectOfCategory.Type.Prefab && (GameObject)x.obj != null)
                                       .Select(x => (GameObject)x.obj).ToArray();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                MeshRenderer[] meshes = gameObjects[i].GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mesh in meshes)
                {
                    foreach (Material mat in mesh.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            string path = AssetDatabase.GetAssetPath(mat);
                            if (!materialPaths.Contains(path))
                            {
                                materialPaths.Add(path);
                            }
                        }
                    }
                }
            }
        }

        void Menu()
        {
            if (EditorPrefs.GetInt("CategorySelected", 0) > pandora.categories.Count - 1)
            {
                EditorPrefs.SetInt("CategorySelected", 0);
            }

            scrollPosBtns = GUILayout.BeginScrollView(scrollPosBtns, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(categoryMenuWidth), GUILayout.Height(this.position.height));
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(categoryMenuWidth - 14));
            {
                float btnWidth = (categoryMenuWidth - 14) / 2;
                GUI.backgroundColor = EditorPrefs.GetInt("tabSelectedInPandora", 0) == 0 ? Color.white / 1.4f : Color.white;

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("GameObject Icon").image, "My Collections"), new GUIStyle(EditorStyles.toolbarButton) { wordWrap = true, stretchWidth = true }, GUILayout.Width(btnWidth)))
                {
                    EditorPrefs.SetInt("tabSelectedInPandora", 0);
                }

                GUI.backgroundColor = EditorPrefs.GetInt("tabSelectedInPandora", 0) == 1 ? Color.white / 1.4f : Color.white;

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Asset Store").image, "Stock"), new GUIStyle(EditorStyles.toolbarButton) { wordWrap = true, stretchWidth = true }, GUILayout.Width(btnWidth)))
                {
                    EditorPrefs.SetInt("tabSelectedInPandora", 1);
                    if (!pandoraAssetsLoaded)
                        LoadCategoriesAndSubCategories();
                }

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            if (currentTab == 0)
            {
                for (int i = 0; i < pandora.categories.Count; i++)
                {
                    DrawCategoryMenuItem(pandora.categories[i].name, i, -1);

                    if (pandora.categories[i].subCategories.Count > 0 && EditorPrefs.GetBool("ShowSubCats" + i, true))
                    {
                        for (int j = 0; j < pandora.categories[i].subCategories.Count; j++)
                        {
                            Pandora.PandoraSubcategory cat = pandora.categories[i].subCategories[j];
                            DrawCategoryMenuItem(cat.name, i, j);
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.white / 1.2f;
                if (GUILayout.Button(new GUIContent(" Add new", EditorGUIUtility.IconContent("P4_AddedRemote").image), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft }))
                {
                    pandora.categories.Add(new Pandora.PandoraCategory("New category"));
                    categoryInEdition = new Vector2(pandora.categories.Count - 1, -1);
                    EditorPrefs.SetInt("CategorySelected", pandora.categories.Count - 1);
                    EditorPrefs.SetInt("SubcategorySelected", -1);
                    GUI.FocusControl(null);
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                GUILine(1, true);
            }
            else if (currentTab == 1)
            {
                for (int i = 0; i < pandora.cloudStockCategories.Length; i++)
                {
                    DrawCloudCategoryMenuItem(pandora.cloudStockCategories[i].name_eng, pandora.cloudStockCategories[i].id, "-1");

                    if (EditorPrefs.GetBool("CloudShowSubCats" + pandora.cloudStockCategories[i].id, true))
                    {
                        for (int j = 0; j < pandora.cloudStockSubCategories.Length; j++)
                        {
                            if (pandora.cloudStockSubCategories[j].parent_category_id == pandora.cloudStockCategories[i].id)
                                DrawCloudCategoryMenuItem(pandora.cloudStockSubCategories[j].name_eng,
                                    pandora.cloudStockCategories[i].id,
                                    pandora.cloudStockSubCategories[j].id);
                        }
                    }
                }
            }

            GUILayout.EndScrollView();

            VerticalResizableArea(ref _currMenuWidth, ref menuIsBeingResized, this);
        }

        void DrawCategoryMenuItem(string name, int catIndex, int subCatIndex)
        {
            GUI.backgroundColor = EditorPrefs.GetInt("CategorySelected", 0) == catIndex && EditorPrefs.GetInt("SubcategorySelected", 0) == subCatIndex ? Color.white / 1.2f : Color.white;

            float height = EditorStyles.toolbar.fixedHeight + 2;
            FontStyle _fontStyle = FontStyle.Normal;

            if (subCatIndex == -1)
            {
                height = 27;
                _fontStyle = FontStyle.Bold;
            }

            GUIStyle st = new GUIStyle(EditorStyles.toolbar) { fixedHeight = height, alignment = TextAnchor.MiddleCenter, fontStyle = _fontStyle };

            Rect r = EditorGUILayout.BeginHorizontal(st, GUILayout.Width(categoryMenuWidth - 14), GUILayout.Height(height));

            if (currentTab == 0)
            {
                Event e = Event.current;
                if (r.Contains(e.mousePosition))
                {
                    if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
                    {
                        catDropIndex = catIndex;
                        subCatDropIndex = subCatIndex;
                    }

                    if (e.type == EventType.MouseUp && e.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Edit"), false, () =>
                        {
                            GUI.FocusControl(null);
                            categoryInEdition = new Vector2(catIndex, subCatIndex);
                        });

                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            if (EditorUtility.DisplayDialog("Warning", "Do you want to delete this category?", "Yes", "Cancel"))
                            {
                                EditorPrefs.GetInt("CategorySelected", 0);
                                EditorPrefs.SetInt("SubcategorySelected", -1);
                                if (subCatIndex == -1)
                                {
                                    pandora.categories.RemoveAt(catIndex);

                                    if (pandora.categories.Count == 0)
                                    {
                                        pandora.categories.Add(new Pandora.PandoraCategory("New category"));
                                    }
                                }
                                else
                                {
                                    pandora.categories[catIndex].subCategories.RemoveAt(subCatIndex);
                                }
                            }
                        });

                        menu.AddSeparator("");

                        if (subCatIndex != -1)
                        {
                            menu.AddItem(new GUIContent("Set as category"), false, () =>
                            {
                                pandora.categories.Add(new Pandora.PandoraCategory(""));
                                Pandora.PandoraCategory cat = pandora.categories[pandora.categories.Count - 1];
                                Pandora.PandoraSubcategory subCat = pandora.categories[catIndex].subCategories[subCatIndex];
                                cat.name = subCat.name;
                                cat.icon = subCat.icon;
                                cat.objects = subCat.objects;
                                pandora.categories[catIndex].subCategories.RemoveAt(subCatIndex);
                            });
                        }
                        else if (subCatIndex == -1)
                        {
                            menu.AddItem(new GUIContent("Add Subcategory"), false, () =>
                            {
                                GUI.FocusControl(null);
                                pandora.categories[catIndex].subCategories.Add(new Pandora.PandoraSubcategory("New Subcategory"));
                                categoryInEdition = new Vector2(catIndex, pandora.categories[catIndex].subCategories.Count - 1);
                                EditorPrefs.SetInt("CategorySelected", catIndex);
                                EditorPrefs.SetInt("SubcategorySelected", pandora.categories[catIndex].subCategories.Count - 1);
                            });
                        }

                        if (subCatIndex != -1 || pandora.categories[catIndex].subCategories.Count == 0)
                        {
                            for (int i = 0; i < pandora.categories.Count; i++)
                            {
                                int x = i; //if we use directly "i", then, it will return the last value in the loop (the number of categories) instead of the current value in the loop.
                                if (x != catIndex)
                                {
                                    menu.AddItem(new GUIContent("Set as subcategory of/" + pandora.categories[x].name), false, () =>
                                    {
                                        pandora.categories[x].subCategories.Add(new Pandora.PandoraSubcategory(""));
                                        Pandora.PandoraSubcategory subCat = pandora.categories[x].subCategories[pandora.categories[x].subCategories.Count - 1];

                                        if (subCatIndex == -1) //is a category
                                        {
                                            Pandora.PandoraCategory cat = pandora.categories[catIndex];
                                            subCat.name = cat.name;
                                            subCat.icon = cat.icon;
                                            subCat.objects = cat.objects;
                                            pandora.categories.RemoveAt(catIndex);
                                        }
                                        else // is subcategory
                                        {
                                            Pandora.PandoraSubcategory currentSubCat = pandora.categories[catIndex].subCategories[subCatIndex];
                                            subCat.name = currentSubCat.name;
                                            subCat.icon = currentSubCat.icon;
                                            subCat.objects = currentSubCat.objects;
                                            pandora.categories[catIndex].subCategories.RemoveAt(subCatIndex);
                                        }
                                    });
                                }
                            }
                        }
                        menu.ShowAsContext();
                        e.Use();
                    }
                }

                if (objsBeingDropped != null && catDropIndex != -1)
                {
                    for (int i = 0; i < objsBeingDropped.Count; i++)
                    {
                        if (PrefabUtility.IsPartOfPrefabAsset(objsBeingDropped[i]))
                        {
                            if (catDropIndex < pandora.categories.Count)
                            {
                                if (subCatDropIndex != -1)
                                {
                                    if (subCatDropIndex < pandora.categories[catDropIndex].subCategories.Count)
                                    {
                                        pandora.categories[catDropIndex].subCategories[subCatDropIndex].objects.Insert(0, new Pandora.PandoraObjectOfCategory(objsBeingDropped[i]));
                                    }
                                }
                                else
                                {
                                    pandora.categories[catDropIndex].objects.Insert(0, new Pandora.PandoraObjectOfCategory(objsBeingDropped[i]));
                                }
                            }
                        }
                    }

                    if (objBeingDragged != null)
                    {
                        objBeingDragged.obj = null;
                        objBeingDragged = null;
                    }

                    objsBeingDropped = null;
                    catDropIndex = -1;
                    subCatDropIndex = -1;
                }

                if (r.Contains(e.mousePosition))
                    objsBeingDropped = RegisterFieldForDrop<GameObject>(r);
            }

            if (subCatIndex != -1)
            {
                GUI.color = Color.gray / 2;
                GUILayout.Button("", new GUIStyle(EditorStyles.toolbarButton) { fixedHeight = height, fixedWidth = 20 }, GUILayout.Width(20), GUILayout.Height(height));
                GUI.color = Color.white;
            }

            if (pandora.categories[catIndex].subCategories.Count > 0 && subCatIndex == -1)
            {
                if (GUILayout.Button(new GUIContent(EditorPrefs.GetBool("ShowSubCats" + catIndex, true) ? EditorGUIUtility.IconContent("d_icon dropdown@2x").image : EditorGUIUtility.IconContent("PlayButton").image, "Show Subcategories"), new GUIStyle(EditorStyles.toolbarButton) { padding = new RectOffset(1, 1, 1, 1), fixedWidth = 20, fixedHeight = height - 1 }, GUILayout.Width(20), GUILayout.Height(height - 1)))
                {
                    EditorPrefs.SetBool("ShowSubCats" + catIndex, !EditorPrefs.GetBool("ShowSubCats" + catIndex, true));
                }
            }

            Texture2D icon;
            EditorGUIUtility.labelWidth = 25;

            if (subCatIndex > pandora.categories[catIndex].subCategories.Count)
            {
                subCatIndex = -1;
                EditorPrefs.SetInt("SubcategorySelected", -1);
            }

            bool editCategoryMode = categoryInEdition == new Vector2(catIndex, subCatIndex) && currentTab == 0;

            if (subCatIndex != -1)
            {
                icon = AssetPreview.GetAssetPreview(pandora.categories[catIndex].subCategories[subCatIndex].icon);
                if (editCategoryMode)
                {
                    pandora.categories[catIndex].subCategories[subCatIndex].icon = EditorGUILayout.ObjectField(pandora.categories[catIndex].subCategories[subCatIndex].icon, typeof(Sprite), false, GUILayout.Width(17)) as Sprite;
                    pandora.categories[catIndex].subCategories[subCatIndex].name = EditorGUILayout.TextField(new GUIContent(icon), pandora.categories[catIndex].subCategories[subCatIndex].name);
                }
            }
            else
            {
                icon = AssetPreview.GetAssetPreview(pandora.categories[catIndex].icon);
                if (editCategoryMode)
                {
                    pandora.categories[catIndex].icon = EditorGUILayout.ObjectField(pandora.categories[catIndex].icon, typeof(Sprite), false, GUILayout.Width(17)) as Sprite;
                    pandora.categories[catIndex].name = EditorGUILayout.TextField(new GUIContent(icon), pandora.categories[catIndex].name);
                }
            }
            EditorGUIUtility.labelWidth = 0;

            if (!editCategoryMode)
            {
                float width = categoryMenuWidth - 14;

                if (subCatIndex != -1
                    || subCatIndex == -1 && pandora.categories[catIndex].subCategories.Count > 0)
                {
                    width -= 15;
                }

                if (GUILayout.Button(new GUIContent(" " + name, icon != null ? icon : EditorGUIUtility.IconContent("Texture Icon").image), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft, fixedHeight = height - 1, fontStyle = _fontStyle }, GUILayout.Width(width), GUILayout.Height(height - 1)))
                {
                    CleanEmptyObjects(catIndex, subCatIndex);

                    EditorPrefs.SetInt("CategorySelected", catIndex);
                    EditorPrefs.SetInt("SubcategorySelected", subCatIndex);
                }
            }
            else
            {
                if ((catIndex > 0 && subCatIndex == -1)
                    || (subCatIndex > 0 && pandora.categories[catIndex].subCategories.Count > 0))
                {
                    if (GUILayout.Button(new GUIContent("▲", "Move Up"), new GUIStyle(EditorStyles.toolbarButton) { fontSize = 8 }, GUILayout.Width(15)))
                    {
                        if (subCatIndex == -1)
                        {
                            MoveElement.Move(pandora.categories, catIndex, catIndex - 1);
                            categoryInEdition.x -= 1;
                            EditorPrefs.SetInt("CategorySelected", (int)categoryInEdition.x);
                            EditorPrefs.SetInt("SubcategorySelected", (int)categoryInEdition.y);
                        }
                        else
                        {
                            MoveElement.Move(pandora.categories[catIndex].subCategories, subCatIndex, subCatIndex - 1);
                            categoryInEdition.y -= 1;
                            EditorPrefs.SetInt("CategorySelected", (int)categoryInEdition.x);
                            EditorPrefs.SetInt("SubcategorySelected", (int)categoryInEdition.y);
                        }
                    }
                }

                if ((catIndex != pandora.categories.Count - 1 && subCatIndex == -1)
                    || (subCatIndex != -1 && subCatIndex != pandora.categories[catIndex].subCategories.Count - 1))
                {
                    if (GUILayout.Button(new GUIContent("▼", "Move Down"), new GUIStyle(EditorStyles.toolbarButton) { fontSize = 8 }, GUILayout.Width(15)))
                    {
                        if (subCatIndex == -1)
                        {
                            MoveElement.Move(pandora.categories, catIndex, catIndex + 1);
                            categoryInEdition.x += 1;
                            EditorPrefs.SetInt("CategorySelected", (int)categoryInEdition.x);
                            EditorPrefs.SetInt("SubcategorySelected", (int)categoryInEdition.y);
                        }
                        else
                        {
                            MoveElement.Move(pandora.categories[catIndex].subCategories, subCatIndex, subCatIndex + 1);
                            categoryInEdition.y += 1;
                            EditorPrefs.SetInt("CategorySelected", (int)categoryInEdition.x);
                            EditorPrefs.SetInt("SubcategorySelected", (int)categoryInEdition.y);
                        }
                    }
                }

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("P4_CheckOutRemote").image), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft }, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    categoryInEdition = new Vector2(-1, -1);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        void DrawCloudCategoryMenuItem(string name, string catID, string subCatID)
        {
            GUI.backgroundColor = EditorPrefs.GetString("CloudCategorySelected", "") == catID
                && EditorPrefs.GetString("CloudSubcategorySelected", "") == subCatID ? Color.white / 1.2f : Color.white;
            float height = EditorStyles.toolbar.fixedHeight + 2;
            FontStyle _fontStyle = FontStyle.Normal;

            if (subCatID == "-1")
            {
                height = 27;
                _fontStyle = FontStyle.Bold;
            }

            GUIStyle st = new GUIStyle(EditorStyles.toolbar) { fixedHeight = height, alignment = TextAnchor.MiddleCenter, fontStyle = _fontStyle };

            Rect r = EditorGUILayout.BeginHorizontal(st, GUILayout.Width(categoryMenuWidth - 14), GUILayout.Height(height));

            if (subCatID != "-1")
            {
                GUI.color = Color.gray / 2;
                GUILayout.Button("", new GUIStyle(EditorStyles.toolbarButton) { fixedHeight = height, fixedWidth = 20 }, GUILayout.Width(20), GUILayout.Height(height));
                GUI.color = Color.white;
            }

            if (pandora.cloudStockCategories.Where(x => x.id == catID).ToArray().Length > 0 && subCatID == "-1")
            {
                if (GUILayout.Button(new GUIContent(EditorPrefs.GetBool("CloudShowSubCats" + catID, true) ? EditorGUIUtility.IconContent("d_icon dropdown@2x").image : EditorGUIUtility.IconContent("PlayButton").image, "Show Subcategories"), new GUIStyle(EditorStyles.toolbarButton) { padding = new RectOffset(1, 1, 1, 1), fixedWidth = 20, fixedHeight = height - 1 }, GUILayout.Width(20), GUILayout.Height(height - 1)))
                {
                    EditorPrefs.SetBool("CloudShowSubCats" + catID, !EditorPrefs.GetBool("CloudShowSubCats" + catID, true));
                }
            }

            Texture2D icon = null;
            EditorGUIUtility.labelWidth = 25;

            //if (subCatID != "-1")
            //{
            //    icon = AssetPreview.GetAssetPreview(pandora.categories[catIndex].subCategory[subCatIndex].icon);
            //}
            //else
            //{
            //    icon = AssetPreview.GetAssetPreview(pandora.categories[catIndex].icon);
            //}

            EditorGUIUtility.labelWidth = 0;

            float width = categoryMenuWidth - 14;

            //if (subCatID != "-1"
            //    || subCatID == "-1" && pandora.categories[catIndex].subCategory.Count > 0)
            //{
            //    width -= 15;
            //}

            if (GUILayout.Button(new GUIContent(" " + name, icon != null ? icon : EditorGUIUtility.IconContent("Texture Icon").image), new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft, fixedHeight = height - 1, fontStyle = _fontStyle }, GUILayout.Width(width), GUILayout.Height(height - 1)))
            {
                if (subCatID == "-1")
                {
                    EditorPrefs.SetBool("CloudShowSubCats" + catID, !EditorPrefs.GetBool("CloudShowSubCats" + catID, true));
                }
                else
                {
                    EditorPrefs.SetString("CloudCategorySelected", catID);
                    EditorPrefs.SetString("CloudSubcategorySelected", subCatID);

                    EditorPrefs.SetInt("CurrentCloudPageSelected", 0);
                }

                LoadAssetsFromCloud(catID, subCatID);
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        void CleanEmptyObjects(int catIndex, int subCatIndex)
        {
            //CLEAN EMPTY OBJECTS
            if (catIndex < pandora.categories.Count)
            {
                if (subCatIndex != -1 && subCatIndex < pandora.categories[catIndex].subCategories.Count)
                {
                    List<Pandora.PandoraObjectOfCategory> subObjs = pandora.categories[catIndex].subCategories[subCatIndex].objects;
                    for (int i = 0; i < subObjs.Count; i++)
                        if (subObjs[i].obj == null) subObjs.RemoveAt(i);
                }
                else
                {
                    List<Pandora.PandoraObjectOfCategory> objs = pandora.categories[catIndex].objects;
                    for (int i = 0; i < objs.Count; i++)
                        if (objs[i].obj == null) objs.RemoveAt(i);
                }
            }
            //-----------------------
        }

        //------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Registers fieldRect for drop operations.
        /// sets the fieldValue to whatever that's been dropped, returns fieldValue in either cases.
        /// </summary>
        static List<T> RegisterFieldForDrop<T>(Rect fieldRect) where T : UnityEngine.Object
        {
            Event e = Event.current;
            EventType eType = e.type;
            if (DragAndDrop.objectReferences.Length > 0)
            {
                if (fieldRect.Contains(e.mousePosition) && eType == EventType.DragUpdated || eType == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (eType == EventType.DragPerform)
                    {
                        List<T> fieldValue = new List<T>();
                        DragAndDrop.AcceptDrag();

                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            if (DragAndDrop.objectReferences[i].GetType() == typeof(T))
                            {
                                fieldValue.Add(DragAndDrop.objectReferences[i] as T);
                            }
                            else if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0])))
                            {
                                //Add all elements of type T of the directory
                                string[] aFilePaths = Directory.GetFiles(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]));
                                foreach (string sFilePath in aFilePaths)
                                {
                                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(sFilePath, typeof(UnityEngine.Object));

                                    if (obj?.GetType() == typeof(T))
                                    {
                                        T objAsset = (T)AssetDatabase.LoadAssetAtPath(sFilePath, typeof(T));

                                        fieldValue.Add(objAsset);
                                    }
                                }
                            }
                        }

                        Event.current.Use();
                        return fieldValue;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Registers fieldRect for drag operations. dragObject is what's being dragged out of that field.
        /// </summary>
        public static void RegisterFieldForDrag(Rect fieldRect, UnityEngine.Object dragObject)
        {
            if (dragObject == null) return; // can't drag a null wtf!
            Event e = Event.current;
            if (fieldRect.Contains(e.mousePosition) && e.type == EventType.MouseDrag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { dragObject };
                DragAndDrop.StartDrag("drag");
                Event.current.Use();
            }
        }

        public static void HorizontalBlock(Action block)
        {
            GUILayout.BeginHorizontal();
            block();
            GUILayout.EndHorizontal();
        }
        //------------------------------------------------------------------------------------------------------------------

        void OnInspectorUpdate()
        {
            Repaint();
            SceneView.RepaintAll();
        }

        void LevelEditorTab()
        {
            if (!maximizeMaterials)
            {
                EditorGUILayout.BeginVertical();
                ToolsLevelEditor(); //Show the tools of the Pandora editor

                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(width - categoryMenuWidth - materialMenuWidth), GUILayout.Height(height - 18));

                if (currentTab == 0)
                {
                    if (pandora.categories.Count > 0)
                    {
                        int category = EditorPrefs.GetInt("CategorySelected", 0);
                        int subCategory = EditorPrefs.GetInt("SubcategorySelected", -1);

                        if (category > pandora.categories.Count - 1)
                        {
                            EditorPrefs.SetInt("CategorySelected", 0);
                            category = 0;
                        }

                        if (subCategory > pandora.categories[category].subCategories.Count - 1)
                        {
                            EditorPrefs.SetInt("SubcategorySelected", -1);
                            subCategory = -1;
                        }

                        if (subCategory != -1)
                        {
                            ShowObjectsList(pandora.categories[category].subCategories[subCategory].objects);
                        }
                        else
                        {
                            ShowObjectsList(pandora.categories[category].objects);
                        }
                    }
                    else
                    {
                        EditorGUIUtility.labelWidth = 300;
                        EditorGUILayout.LabelField("Plese create a category.", EditorStyles.boldLabel);
                    }
                }
                else
                {
                    ShowCloudObjectsList(EditorPrefs.GetString("CloudCategorySelected", ""), EditorPrefs.GetString("CloudSubcategorySelected", ""));
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        void Deselect()
        {
            objSelectedIsUI = false;
            objSelected = null;
            if (objInHandle != null)
                DestroyImmediate(objInHandle);
            objInHandle = null;

            ChangeToolToRect();
        }

        void ToolsLevelEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                CategorySelection();

                if (objSelected != null)
                {
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    {
                        EditorGUIUtility.labelWidth = 20;

                        if (!objSelectedIsUI)
                        {
                            if (objSelected != null)
                            {
                                if (GUILayout.Button(new GUIContent(" DESELECT", EditorGUIUtility.IconContent("d_scenepicking_notpickable_hover").image), EditorStyles.toolbarButton, GUILayout.Width(90), GUILayout.Height(20)))
                                {
                                    Deselect();
                                }
                            }

                            if (GUILayout.Button(lookAtCameraTexture, EditorStyles.toolbarButton, GUILayout.Width(30), GUILayout.Height(25)))
                            {
                                LookAtCamera();
                            }

                            if (EditorPrefs.GetBool("_ShowGrid", false))
                                GUI.backgroundColor = Color.gray;
                            else
                                GUI.backgroundColor = Color.white;

                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("UnityEditor.SceneView").image, "Show Grid"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                            {
                                if (!EditorPrefs.GetBool("_ShowGrid", false))
                                {
                                    CreateGrid();
                                    Selection.activeObject = grid;
                                    EditorPrefs.SetBool("_ShowGrid", true);
                                }
                                else
                                {
                                    Selection.activeObject = null;
                                    EditorPrefs.SetBool("_ShowGrid", false);
                                }
                            }

                            if (EditorPrefs.GetBool("_ShowGrid", true))
                            {
                                EditorGUIUtility.labelWidth = 45;

                                if (grid == null)
                                {
                                    EditorPrefs.SetBool("_ShowGrid", false);
                                    return;
                                }

                                if (grid.transform.position != Vector3.zero)
                                    grid.transform.position = Vector3.zero;

                                if (grid.cellLayout == GridLayout.CellLayout.Rectangle)
                                    grid.cellGap = Vector3.zero;

                                grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

                                if (Selection.activeObject != grid && objSelected != null)
                                {
                                    Selection.activeObject = grid;
                                }
                                else if (Selection.activeObject != grid && objSelected == null)
                                {
                                    EditorPrefs.SetBool("_ShowGrid", false);
                                }
                            }
                            else
                            {
                                EditorPrefs.SetBool("FitWithGrid", false);
                            }

                            if (EditorPrefs.GetBool("FitWithGrid", false))
                                GUI.backgroundColor = Color.gray;
                            else
                                GUI.backgroundColor = Color.white;

                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SceneViewSnap").image, "Snap To Grid"), EditorStyles.toolbarButton, GUILayout.Width(35)))
                            {
                                if (!EditorPrefs.GetBool("_ShowGrid", false))
                                {
                                    Selection.activeObject = grid;
                                    EditorPrefs.SetBool("_ShowGrid", true);
                                }

                                if (!EditorPrefs.GetBool("FitWithGrid", false))
                                {
                                    EditorPrefs.SetBool("FitWithGrid", true);
                                }
                                else
                                {
                                    EditorPrefs.SetBool("FitWithGrid", false);
                                }
                            }
                            GUI.backgroundColor = Color.white;

                            EditorGUIUtility.labelWidth = 40;

                            if ((GameObject)objSelected != null)
                            {
                                if (((GameObject)objSelected).transform.position.z != zPosToInstantiate)
                                {
                                    zPosToInstantiate = ((GameObject)objSelected).transform.position.z;
                                }

                                SetZPosToObject(((GameObject)objSelected).transform, EditorGUILayout.FloatField("Pos Z:", zPosToInstantiate, GUILayout.Width(85)));

                                EditorGUIUtility.labelWidth = 50;
                                objectSizeMultiplier = EditorGUILayout.Slider("| Size: ", objectSizeMultiplier, 0.1f, 10);
                                if (GUILayout.Button("Original Size", EditorStyles.toolbarButton, GUILayout.Width(80)))
                                {
                                    objectSizeMultiplier = 1;
                                    GUIUtility.keyboardControl = 0;
                                }
                                EditorGUILayout.LabelField("Parent", GUILayout.Width(43));
                                pandora.showParent = EditorGUILayout.Toggle(pandora.showParent, GUILayout.Width(15));
                                if (pandora.showParent)
                                {
                                    pandora.parent = EditorGUILayout.ObjectField(pandora.parent, typeof(Transform), true) as Transform;
                                }
                                else
                                {
                                    pandora.parent = null;
                                }

                                if (pandora.parent != null && pandora.parent.GetComponent<RectTransform>() != null)
                                {
                                    pandora.parent = null;
                                    pandora.showParent = false;
                                }
                            }
                        }
                        else
                        {
                            EditorGUIUtility.labelWidth = 75;
                            pandora.parent = EditorGUILayout.ObjectField("Canvas UI: ", pandora.parent, typeof(RectTransform), true) as RectTransform;

                            if (pandora.parent == null)
                            {
                                if (GUILayout.Button("Create New Canvas", EditorStyles.miniButton, GUILayout.Width(120)))
                                {
                                    // Create Button
                                    EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");

                                    // Get GameObject of Button
                                    GameObject go = Selection.activeGameObject;

                                    if (go != null)
                                    {
                                        pandora.parent = go.GetComponent<RectTransform>();
                                    }

                                    EditorGUIUtility.PingObject(go);
                                    if (objInHandle != null)
                                        DestroyImmediate(objInHandle);

                                    objSelected = null;
                                    ChangeToolToRect();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        static void LookAtCamera()
        {
            if (FindObjectOfType<Camera>() != null)
            {
                SceneView.lastActiveSceneView.LookAt(FindObjectOfType<Camera>().transform.position);
            }
            else
            {
                Debug.Log("<color=yellow>Expiria3D: </color>You do not have a camera in the scene.");
            }
        }

        void ShowObjectsList(List<Pandora.PandoraObjectOfCategory> objects)
        {
            if (objects.Count < 1)
            {
                Rect rect = EditorGUILayout.BeginVertical("box", GUILayout.Height(this.height - 30));
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(new GUIContent(" Drag and drop prefabs here.", EditorGUIUtility.IconContent("Prefab Icon").image), new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 20 });

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();

                if (currentTab == 0)
                {
                    //VALIDATE DROP ELEMENTS----------------------------------------
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        objsBeingDropped = RegisterFieldForDrop<GameObject>(rect);
                        if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
                        {
                            if (rect.Contains(Event.current.mousePosition) && objsBeingDropped != null)
                            {
                                for (int objIndex = 0; objIndex < objsBeingDropped.Count; objIndex++)
                                {
                                    if (PrefabUtility.IsPartOfPrefabAsset(objsBeingDropped[objIndex]))
                                        objects.Add(new Pandora.PandoraObjectOfCategory(objsBeingDropped[objIndex]));
                                }
                                objsBeingDropped = null;
                            }
                        }
                    }
                    //END VALIDATE DROP ELEMENTS----------------------------------------
                }
                return;
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(4);

            bool horizontalEnded = true;

            float windowWidth = (this.position.width - categoryMenuWidth - materialMenuWidth) - 14 - 18; //14 is the scrollbar, 18 is other UI elements
            windowWidth -= (int)(windowWidth / pandora.boxSize) * 5; //5 is an additional space between the elements.
            int cols = (int)(windowWidth / pandora.boxSize); //width is the fixedWidth of every element.
            if (cols == 0) cols = 1;
            int numObjects = 0; //used becouse there would be null objects

            float size = (windowWidth) / cols;
            if (size > 175)
                size = 175;
            else if (size < 65)
                size = 65;

            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 9, fixedHeight = 25 };
            GUIStyle itemStyle = new GUIStyle(EditorStyles.toolbarButton);

            itemStyle.fixedWidth = size;

            if (!pandora.showNames)
            {
                itemStyle.fixedHeight = size - (16 * size / 100);
            }
            else
            {
                itemStyle.fixedHeight = size;

                if (size > 100)
                {
                    textStyle.alignment = TextAnchor.MiddleCenter;
                }
                else
                    textStyle.alignment = TextAnchor.UpperCenter;
            }

            float imgSize = size * 0.75f;

            using (new GUILayout.VerticalScope("framebox", GUILayout.Height(20)))
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].obj != null && (!showFavorites || (showFavorites && objects[i].isFavorite)))
                    {
                        if (objects[i].obj.name.ToLower().Contains(textToSearch.ToLower()))
                        {
                            if (numObjects != 0 && numObjects % cols == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                horizontalEnded = true;
                            }

                            if (numObjects % cols == 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                horizontalEnded = false;
                            }

                            numObjects++;

                            if (objSelected != objects[i].obj)
                                GUI.backgroundColor = Color.white;
                            else
                                GUI.backgroundColor = Color.green;

                            if (objects[i].assetPreview == null)
                            {
                                if (!AssetPreview.IsLoadingAssetPreviews())
                                {
                                    objects[i].assetPreview = GetPreviewTextureWithAlpha(objects[i].obj);
                                    if (objects[i].type == Pandora.PandoraObjectOfCategory.Type.Prefab && objects[i].obj != null && objects[i].obj is GameObject)
                                    {
                                        if (((GameObject)objects[i].obj).GetComponent<RectTransform>() != null)
                                            objects[i].isUI = true;
                                    }
                                }
                            }

                            //-------------------------------------------------------------------
                            GUILayout.Space(5);
                            EditorGUILayout.BeginVertical(GUILayout.Width(size));
                            GUILine(1);
                            Rect rect = EditorGUILayout.BeginVertical(itemStyle);

                            //FAVORITE AND DELETE BUTTONS --------------------------------------------------
                            if (!removeElementsMode)
                            {
                                bool isFavorite = objects[i].isFavorite;
                                GUI.color = isFavorite ? Color.white : Color.gray;

                                if (GUI.Button(new Rect(rect.x, rect.y, 15, 15), EditorGUIUtility.IconContent(isFavorite ? "Favorite On Icon" : "Favorite Icon", "Favorite"), EditorStyles.iconButton))
                                {
                                    objects[i].isFavorite = isFavorite ? false : true;
                                }
                            }
                            GUI.color = Color.white;
                            if (removeElementsMode)
                            {
                                if (GUI.Button(new Rect(rect.x + rect.size.x - 19, rect.y + 2, 15, 15), new GUIContent(EditorGUIUtility.IconContent("CollabDeleted Icon").image, "Remove"), EditorStyles.iconButton))
                                {
                                    objects.RemoveAt(i);
                                    GUIUtility.ExitGUI();
                                }
                            }
                            //END FAVORITE AND DELETE BUTTONS --------------------------------------------------

                            bool elementClicked = RegisterFieldForDrag(rect, objects[i]);
                            {
                                //VALIDATE DROP ELEMENTS----------------------------------------
                                Rect leftRect = new Rect(rect.position, new Vector2(rect.size.x / 3, rect.size.y));
                                Rect rightRect = new Rect(new Vector2(rect.position.x + rect.size.x / 1.2f, rect.position.y), new Vector2(rect.size.x / 3, rect.size.y));

                                if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
                                {
                                    if (leftRect.Contains(Event.current.mousePosition))
                                    {
                                        GUI.DrawTexture(new Rect(new Vector2(rect.position.x - 4, rect.position.y), new Vector2(2, rect.size.y)), Texture2D.whiteTexture);
                                        dropIndex = i;
                                    }

                                    if (rightRect.Contains(Event.current.mousePosition))
                                    {
                                        GUI.DrawTexture(new Rect(new Vector2(rect.position.x + rect.size.x, rect.position.y), new Vector2(2, rect.size.y)), Texture2D.whiteTexture);
                                        dropIndex = i + 1;
                                    }
                                }

                                if (objsBeingDropped != null && dropIndex != -1)
                                {
                                    for (int objIndex = 0; objIndex < objsBeingDropped.Count; objIndex++)
                                    {
                                        if (PrefabUtility.IsPartOfPrefabAsset(objsBeingDropped[objIndex]))
                                        {
                                            objects.Insert(dropIndex, new Pandora.PandoraObjectOfCategory(objsBeingDropped[objIndex]));
                                        }
                                    }

                                    if (objBeingDragged != null)
                                    {
                                        objBeingDragged.obj = null;
                                        objBeingDragged = null;
                                    }

                                    dropIndex = -1;
                                    objsBeingDropped = null;
                                }

                                if (rect.Contains(Event.current.mousePosition))
                                {
                                    objsBeingDropped = RegisterFieldForDrop<GameObject>(leftRect);

                                    if (objsBeingDropped == null)
                                        objsBeingDropped = RegisterFieldForDrop<GameObject>(rightRect);
                                }
                                //END VALIDATE DROP ELEMENTS----------------------------------------

                                string name = objects[i].obj == null ? "" : objects[i].obj.name;
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                GUILayout.Label(new GUIContent(objects[i].assetPreview == null ? AssetPreview.GetMiniThumbnail(objects[i].obj) : objects[i].assetPreview, name), new GUIStyle("label") { fixedHeight = imgSize, alignment = TextAnchor.MiddleCenter }, GUILayout.Height(imgSize), GUILayout.Width(imgSize));
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndHorizontal();

                                if (pandora.showNames)
                                    GUILayout.Label(new GUIContent(name, name), textStyle);
                            }
                            EditorGUILayout.EndVertical();

                            GUILine(1, true);
                            EditorGUILayout.EndVertical();
                            //--------------------------------------------------------------------------------

                            if (elementClicked && !removeElementsMode)
                            {
                                if (objects[i].isUI)
                                {
                                    objSelectedIsUI = true;
                                }
                                else
                                {
                                    objSelectedIsUI = false;
                                }

                                if (objSelectedIsUI)
                                    LookAtCamera();

                                if (objSelected == objects[i].obj)
                                {
                                    Deselect();
                                }
                                else
                                {
                                    //if scene view is in 2D mode
                                    if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.in2DMode)
                                    {
                                        objSelected = objects[i].obj;
                                        if (((GameObject)objInHandle) != null)
                                        {
                                            DestroyImmediate(((GameObject)objInHandle));
                                        }
                                        objInHandle = null;

                                        zPosToInstantiate = ((GameObject)objects[i].obj).transform.position.z;
                                        ChangeToolToView();

                                        objectSizeMultiplier = 1;
                                        GUIUtility.keyboardControl = 0;
                                    }
                                    else
                                    {
                                        objSelected = null;

                                        if (((GameObject)objInHandle) != null)
                                            DestroyImmediate(((GameObject)objInHandle));

                                        objInHandle = null;
                                    }
                                }
                            }
                        }

                    }
                    else if (objects[i].obj == null)
                    {
                        //NOTE: If we delete the element here, Unity throw exception of GUI
                        //So, we delete them when the OnGUI starts.
                        hasEmptyObjects = true;
                    }
                }
                GUI.backgroundColor = Color.white;

                if (!horizontalEnded)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void ShowCloudObjectsList(string catID, string subCatID)
        {
            string subCategory = EditorPrefs.GetString("CloudSubcategorySelected", "");
            string category = EditorPrefs.GetString("CloudCategorySelected", "");

            //if the selected sub-category is null
            if ((subCategory == "" || subCategory == "-1") && category != "")
            {
                Pandora.CloudStockSubCategory subCat = pandora.cloudStockSubCategories.FirstOrDefault(x => x.parent_category_id == category);
                if (subCat != null)
                {
                    EditorPrefs.SetString("CloudSubcategorySelected", subCat.id);
                    subCatID = subCat.id;
                }
            }

            if (Pandora.instance.cloudAssetsInView == null || Pandora.instance.cloudAssetsInView.Length == 0)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Height(this.height - 30));
                {
                    GUILayout.FlexibleSpace();
                    {
                        EditorGUILayout.LabelField("¯\\_(ツ)_/¯ Oops... Please reload the content.", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 20 });
                        GUILayout.Space(7);
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            {
                                if (GUILayout.Button(new GUIContent(" Reload content.", EditorGUIUtility.IconContent("Refresh@2x").image), new GUIStyle(EditorStyles.miniButton) { fontSize = 17, fixedHeight = 50, fixedWidth = 270 }))
                                {
                                    LoadAssetsFromCloud(catID, subCatID);
                                }
                            }
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndVertical();

                return;
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(4);

            bool horizontalEnded = true;

            float windowWidth = (this.position.width - categoryMenuWidth - materialMenuWidth) - 14 - 18; //14 is the scrollbar, 18 is other UI elements
            windowWidth -= (int)(windowWidth / 150) * 5; //5 is an additional space between the elements.
            int cols = (int)(windowWidth / 150); //width is the fixedWidth of every element.
            if (cols == 0) cols = 1;
            int numObjects = 0; //used becouse there would be null objects

            float size = (windowWidth) / cols;
            float height = size * 0.6f;
            if (size > 175)
                size = 175;
            else if (size < 65)
                size = 65;

            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 9, fixedHeight = 25 };
            GUIStyle itemStyle = new GUIStyle(EditorStyles.toolbarButton);


            itemStyle.fixedWidth = size;

            if (!pandora.showNames)
            {
                itemStyle.fixedHeight = size - (16 * size / 100);
            }
            else
            {
                itemStyle.fixedHeight = size * 0.72f;
                textStyle.alignment = TextAnchor.MiddleCenter;
            }

            using (new GUILayout.VerticalScope("framebox", GUILayout.Height(20)))
            {
                int currentCloudPageSelected = EditorPrefs.GetInt("CurrentCloudPageSelected", 0);

                List<Pandora.CloudStockAsset> filteredAssets = new List<Pandora.CloudStockAsset>();
                for (int i = 0; i < Pandora.instance.cloudAssetsInView.Length; i++)
                {
                    if (!showFavorites || (showFavorites && StockAssetIsFavorite(Pandora.instance.cloudAssetsInView[i].id)))
                    {
                        if (!showAnimatedInStock ||
                            (showAnimatedInStock && Pandora.instance.cloudAssetsInView[i].animation_names != "_NULL_" && !string.IsNullOrEmpty(Pandora.instance.cloudAssetsInView[i].animation_names)))
                        {
                            bool containsSearch = Pandora.instance.cloudAssetsInView[i].name_eng.ToLower().Contains(textToSearch.ToLower())
                                || Pandora.instance.cloudAssetsInView[i].name_esp.ToLower().Contains(textToSearch.ToLower())
                                || Pandora.instance.cloudAssetsInView[i].tags_eng.ToLower().Contains(textToSearch.ToLower())
                                || Pandora.instance.cloudAssetsInView[i].tags_esp.ToLower().Contains(textToSearch.ToLower());

                            if (containsSearch)
                            {
                                filteredAssets.Add(Pandora.instance.cloudAssetsInView[i]);
                            }
                        }
                    }
                }

                totalPagesInStock = (int)(filteredAssets.Count / 40f);
                if ((filteredAssets.Count / 40f) % 1 == 0)
                {
                    if (totalPagesInStock > 0)
                        totalPagesInStock--;
                }

                if (currentCloudPageSelected > totalPagesInStock)
                {
                    EditorPrefs.SetInt("CurrentCloudPageSelected", totalPagesInStock);
                    currentCloudPageSelected = totalPagesInStock;
                }

                int from = currentCloudPageSelected * 40;
                int to;

                if (currentCloudPageSelected == totalPagesInStock)
                {
                    to = filteredAssets.Count;
                }
                else
                {
                    to = (currentCloudPageSelected + 1) * 40;
                }

                if (filteredAssets.Count == 0)
                {
                    EditorGUILayout.LabelField("No elements found with your filters or search.", EditorStyles.boldLabel);
                }

                for (int i = from; i < to; i++)
                {

                    if (numObjects != 0 && numObjects % cols == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        horizontalEnded = true;
                    }

                    if (numObjects % cols == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        horizontalEnded = false;
                    }

                    numObjects++;

                    //-------------------------------------------------------------------
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(GUILayout.Width(size));
                    {
                        GUILine(1);
                        Rect rect = EditorGUILayout.BeginVertical(itemStyle);
                        {
                            //FAVORITE AND DOWNLOAD BUTTONS --------------------------------------------------

                            GUI.Label(new Rect(rect.x + 20, rect.y - 2, size - 40, 15), new GUIContent(filteredAssets[i].name_eng, filteredAssets[i].name_eng), textStyle);
                            bool isFavorite = StockAssetIsFavorite(filteredAssets[i].id);
                            GUI.color = isFavorite ? Color.white : Color.gray;

                            if (GUI.Button(new Rect(rect.x, rect.y, 15, 15), EditorGUIUtility.IconContent(isFavorite ? "Favorite On Icon" : "Favorite Icon", "Favorite"), EditorStyles.iconButton))
                            {
                                isFavorite = !isFavorite;

                                List<Pandora.LikedAssetsInStock> likedAssets = Pandora.instance.likedAssetsInStock.ToList();

                                if (isFavorite)
                                    likedAssets.Add(new Pandora.LikedAssetsInStock(filteredAssets[i].id));
                                else
                                    likedAssets.Remove(likedAssets.FirstOrDefault(x => x.asset_id == filteredAssets[i].id));

                                Pandora.instance.likedAssetsInStock = likedAssets.ToArray();

                                string type = isFavorite ? "add" : "remove";
                                SendPandoraBackendRequest("set_star¦" + type + "¦" + filteredAssets[i].id);
                            }

                            GUI.color = Color.white;

                            if (GUI.Button(new Rect(rect.x + rect.size.x - 19, rect.y + 2, 15, 15), new GUIContent(EditorGUIUtility.IconContent("Download-Available").image, "Download Now"), EditorStyles.iconButton))
                            {
                                string jsonForAssets = SendPandoraBackendRequest("download¦" + filteredAssets[i].id);

                                if (!string.IsNullOrEmpty(jsonForAssets))
                                {
                                    string downloadUrl = "https://firebasestorage.googleapis.com/v0/b/ugame-studio-pandora/o/unitypackage_i9ADK_u1%2F" + jsonForAssets + "?alt=media";

                                    downloadedAssetID = filteredAssets[i].id;
                                    downloadedFromCatID = catID;
                                    downloadedFromSubCatID = subCatID;

                                    PandoraDownloadManager.StartBackgroundTask(PandoraDownloadManager.DownloadFile(downloadUrl, false));

                                    GUIUtility.ExitGUI();
                                }
                            }

                            //END FAVORITE AND DOWNLOAD BUTTONS --------------------------------------------------

                            Rect imgRect = EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.FlexibleSpace();

                                if (filteredAssets[i].imageURLs.Length > 0)
                                {
                                    bool isTextureNull = filteredAssets[i].texturePreviews[0] == null || (filteredAssets[i].texturePreviews[0] != null && filteredAssets[i].texturePreviews[0].width == 1);
                                    if (!String.IsNullOrEmpty(filteredAssets[i].imageURLs[0]) && isTextureNull && !filteredAssets[i].isLoadingPreviews)
                                    {
                                        StartBackgroundTask(UpdateTextureFromCloud(filteredAssets[i], 0));
                                    }

                                    if (filteredAssets[i].texturePreviews[0] != null)
                                    {
                                        string tooltip =
                                            "Name: " + filteredAssets[i].name_eng + "\r\n\r\n" +
                                            "License: " + filteredAssets[i].license + "\r\n\r\n" +
                                            "Author: " + filteredAssets[i].author + "\r\n\r\n" +
                                            "Tags: " + filteredAssets[i].tags_eng + "\r\n\r\n" +
                                            filteredAssets[i].asset_details.Replace(", Textures: 0", "").Replace("//", "\r\n") + "\r\n\r\n" +
                                            "Assets: " + filteredAssets[i].asset_names_to_add_in_collections;

                                        if (filteredAssets[i].animation_names != "_NULL_")
                                        {
                                            tooltip += "\r\n\r\n" + "Animations: " + filteredAssets[i].animation_names;
                                        }
                                        tooltip += "\r\n";
                                        GUI.Label(new Rect(rect.x - 2, rect.y + 20, size + 2, height), new GUIContent(
                                        filteredAssets[i].texturePreviews[0],
                                        tooltip));
                                    }
                                }
                                GUILayout.FlexibleSpace();
                            }
                            EditorGUILayout.EndHorizontal();
                            imgRect.height = size * 0.72f;
                            if (filteredAssets[i].texturePreviews.Length > 0)
                            {
                                if (imgRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                                {
                                    PopupWindow.Show(new Rect(Event.current.mousePosition, imgRect.size), new ShowPreviewImage(filteredAssets[i].texturePreviews[0]));
                                }
                            }

                            if (filteredAssets[i].animation_names != "_NULL_")
                            {
                                string[] animations = filteredAssets[i].animation_names.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                if (animations.Length > 0)
                                {
                                    GUI.Label(new Rect(rect.x, rect.y + rect.height - 20, rect.width, 20), new GUIContent("x" + animations.Length, EditorGUIUtility.IconContent("AnimationClip Icon").image));
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        GUILine(1, true);
                    }
                    EditorGUILayout.EndVertical();
                    //--------------------------------------------------------------------------------
                }
                GUI.backgroundColor = Color.white;

                if (!horizontalEnded)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void ClearTexturesToClearMemory()
        {
            for (int i = 0; i < Pandora.instance.cloudAssetsInView.Length; i++)
            {
                Pandora.instance.cloudAssetsInView[i].ClearTexturesToClearMemory();
            }
        }

        bool StockAssetIsFavorite(string id)
        {
            return Pandora.instance.likedAssetsInStock.FirstOrDefault(x => x.asset_id == id) != null;
        }

        public class ShowPreviewImage : PopupWindowContent
        {
            Texture2D textureToShow;

            public override Vector2 GetWindowSize()
            {
                return new Vector2(488, 290);
            }

            public ShowPreviewImage(Texture2D texture)
            {
                this.textureToShow = texture;
            }

            public override void OnGUI(Rect rect)
            {
                if (textureToShow != null)
                    GUI.Label(rect, textureToShow);

                if (GUI.Button(new Rect(rect.x + rect.width - 20, 5, 20, 20), EditorGUIUtility.IconContent("winbtn_mac_close_h").image, EditorStyles.iconButton))
                {
                    editorWindow.Close();
                }
            }
        }
        void StartBackgroundTask(IEnumerator update, Action end = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        if (end != null)
                            end();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch
                {
                    if (end != null)
                        end();

                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }

        static IEnumerator UpdateTextureFromCloud(Pandora.CloudStockAsset assetToSaveTheTexture, int textureID)
        {
            assetToSaveTheTexture.isLoadingPreviews = true;

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(assetToSaveTheTexture.imageURLs[textureID]);

            yield return www.SendWebRequest();

            assetToSaveTheTexture.texturePreviews[textureID] = new Texture2D(1, 1);

            while (!www.isDone)
            {
                yield return www;
            }

            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
            {
                //Debug.Log("<color=yellow>Expiria3D: </color>Error loading an image for Templates and Resources. It was due to: " + www.error);
                assetToSaveTheTexture.texturePreviews[textureID] = null;
            }
            else
            {
                assetToSaveTheTexture.texturePreviews[textureID] = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }

            assetToSaveTheTexture.isLoadingPreviews = false;

            www.Dispose();
        }
        void RefreshIcons()
        {
            int cat = EditorPrefs.GetInt("CategorySelected", 0);
            int subCat = EditorPrefs.GetInt("SubcategorySelected", 0);
            List<Pandora.PandoraObjectOfCategory> objects;

            if (subCat == -1)
            {
                objects = pandora.categories[cat].objects;
            }
            else
            {
                objects = pandora.categories[cat].subCategories[subCat].objects;
            }

            for (int objs = 0; objs < objects.Count; objs++)
            {
                if (objects[objs].obj != null)
                {
                    objects[objs].assetPreview = null;

                    if ((GameObject)objects[objs].obj != null)
                    {
                        if (((GameObject)objects[objs].obj).GetComponent<RectTransform>() != null)
                            objects[objs].isUI = true;
                    }
                }
                else
                {
                    objects.RemoveAt(objs);
                }
            }
            Repaint();
        }

        void CategorySelection()
        {
            if (objSelected == null)
            {
                using (new GUILayout.HorizontalScope("toolbar"))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = collapseMenu ? Color.white : Color.white / 1.4f;
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow").image, "Collapse Menu"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        collapseMenu = !collapseMenu;
                    }

                    //GUI.backgroundColor = collapseMaterialMenu ? Color.white : Color.white / 1.4f;
                    //if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Material Icon").image, "Collapse Materials"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    //{
                    //    collapseMaterialMenu = !collapseMaterialMenu;
                    //}

                    GUI.backgroundColor = !showFavorites ? Color.white : Color.cyan / 1.1f;
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(showFavorites ? "Favorite On Icon" : "Favorite Icon").image, "Show Favorites Only"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        showFavorites = !showFavorites;
                        textToSearch = "";
                        GUI.FocusControl(null);
                    }
                    GUI.backgroundColor = Color.white;

                    if (currentTab == 1)
                    {
                        GUI.backgroundColor = !showAnimatedInStock ? Color.white : Color.cyan / 1.1f;
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("AnimationClip Icon").image, "Show Animated Only"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            showAnimatedInStock = !showAnimatedInStock;
                            textToSearch = "";
                            GUI.FocusControl(null);
                        }
                        GUI.backgroundColor = Color.white;
                    }

                    EditorGUIUtility.labelWidth = 52;

                    string prevText = textToSearch;

                    textToSearch = EditorGUILayout.TextField("Search: ", textToSearch, ToolbarSearchBar.SearchBarStyle);

                    if (GUILayout.Button("", ToolbarSearchBar.CancelBtnStyle))
                    {
                        // Remove focus if cleared
                        textToSearch = "";
                        GUI.FocusControl(null);
                    }

                    if (textToSearch != prevText)
                    {
                        ClearTexturesToClearMemory();
                    }

                    EditorGUILayout.EndHorizontal();

                    //make an space
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                    //-------------

                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(110));

                    if (currentTab == 0)
                    {
                        GUI.backgroundColor = !removeElementsMode ? Color.white : Color.red / 1.1f;

                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("CollabDeleted Icon").image, "Remove Elements Mode"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            removeElementsMode = !removeElementsMode;
                        }

                        GUI.backgroundColor = Color.white;
                    }

                    if (currentTab == 1)
                    {
                        if (EditorPrefs.GetInt("CurrentCloudPageSelected") < 0)
                        {
                            EditorPrefs.SetInt("CurrentCloudPageSelected", 0);
                        }

                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev").image, EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            if (EditorPrefs.GetInt("CurrentCloudPageSelected") > 0)
                                EditorPrefs.SetInt("CurrentCloudPageSelected", EditorPrefs.GetInt("CurrentCloudPageSelected") - 1);

                            scrollPos = new Vector2();

                            ClearTexturesToClearMemory();
                        }

                        EditorGUILayout.LabelField((EditorPrefs.GetInt("CurrentCloudPageSelected") + 1) + "/" + (totalPagesInStock + 1), EditorStyles.boldLabel, GUILayout.Width(37));

                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_next").image, EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            EditorPrefs.SetInt("CurrentCloudPageSelected", EditorPrefs.GetInt("CurrentCloudPageSelected") + 1);
                            scrollPos = new Vector2();

                            ClearTexturesToClearMemory();
                        }
                    }

                    if (currentTab == 0)
                    {
                        pandora.boxSize = (int)GUILayout.HorizontalSlider(pandora.boxSize, 65, 150, GUILayout.Width(60));

                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SettingsIcon").image, "Settings"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            // create the menu and add items to it
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Show Names"), pandora.showNames, () => { pandora.showNames = !pandora.showNames; });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Refresh Icons"), false, () => { RefreshIcons(); });
                            // display the menu
                            menu.ShowAsContext();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void SetZPosToObject(Transform objToSetPosition, float newZ)
        {
            Vector3 newPos = new Vector3(objToSetPosition.position.x, objToSetPosition.position.y, newZ);
            objToSetPosition.position = newPos;
        }

        Texture2D GetPreviewTextureWithAlpha(UnityEngine.Object unityObject)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(unityObject);

            if (tex != null)
                PixelsToAlpha(tex, tex);

            return tex;
        }

        void PixelsToAlpha(Texture2D oldTex, Texture2D newTex)
        {
            Color colorToReplace = oldTex.GetPixel(0, 0);
            int[,] pixelsToReplace = new int[oldTex.width, oldTex.height];
            for (int x = 0; x < newTex.width; x++)
            {
                for (int y = 0; y < newTex.height; y++)
                {
                    bool[] neightbourSameColor = new bool[4] { false, false, false, false };
                    neightbourSameColor[0] = x - 1 > 0 ? oldTex.GetPixel(x - 1, y) == colorToReplace : false;
                    neightbourSameColor[1] = x + 1 < oldTex.width ? oldTex.GetPixel(x + 1, y) == colorToReplace : false;
                    neightbourSameColor[2] = y - 1 > 0 ? oldTex.GetPixel(x, y - 1) == colorToReplace : false;
                    neightbourSameColor[3] = y + 1 < oldTex.height ? oldTex.GetPixel(x, y + 1) == colorToReplace : false;
                    pixelsToReplace[x, y] = oldTex.GetPixel(x, y) == colorToReplace && neightbourSameColor.Contains(true) ? 1 : 0;
                }
            }
            for (int x = 0; x < newTex.width; x++)
            {
                for (int y = 0; y < newTex.height; y++)
                {
                    Color col = pixelsToReplace[x, y] == 1 ? Color.clear : oldTex.GetPixel(x, y);
                    newTex.SetPixel(x, y, col);
                }
            }
            newTex.Apply();
        }

        void VerticalResizableArea(ref float currentWidth, ref bool isResizing, EditorWindow editorWindow, float additionalWidth = 0, float maxWidth = -1, float fixedHeight = -1, float minWidth = 35)
        {
            float w = currentWidth + additionalWidth;
            float h = (fixedHeight == -1) ? editorWindow.position.height : fixedHeight;

            Rect cursorChangeRect = new Rect(w, 0, 2, h);

            if (maxWidth == -1)
                maxWidth = editorWindow.position.width / 1.5f;

            GUI.DrawTexture(cursorChangeRect, Texture2D.grayTexture);
            EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && cursorChangeRect.Contains(Event.current.mousePosition))
            {
                isResizing = true;
            }

            if (isResizing)
            {
                w = Event.current.mousePosition.x;
                if (w > additionalWidth + maxWidth)
                    w = additionalWidth + maxWidth;
                else if (w < additionalWidth + minWidth)
                    w = additionalWidth + minWidth;

                cursorChangeRect.Set(w, cursorChangeRect.y, cursorChangeRect.width, cursorChangeRect.height);
                currentWidth = w - additionalWidth;
                editorWindow.Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
                isResizing = false;
        }
    }

    public static class MoveElement
    {
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            // exit if positions are equal or outside array
            if ((oldIndex == newIndex) || (0 > oldIndex) || (oldIndex >= list.Count) || (0 > newIndex) ||
                (newIndex >= list.Count)) return;
            // local variables
            var i = 0;
            T tmp = list[oldIndex];
            // move element down and shift other elements up
            if (oldIndex < newIndex)
            {
                for (i = oldIndex; i < newIndex; i++)
                {
                    list[i] = list[i + 1];
                }
            }
            // move element up and shift other elements down
            else
            {
                for (i = oldIndex; i > newIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }
            // put element from position 1 to destination
            list[newIndex] = tmp;
        }
    }

}
