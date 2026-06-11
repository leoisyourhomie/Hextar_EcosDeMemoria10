namespace Expiria3D
{
    using UnityEditor;
    using UnityEngine;
    using ExpiriaToolbarExtender;
    using System.Reflection;
    using Expiria3DSpace;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    [InitializeOnLoad]
    public static class UGSArButtonInToolbar
    {
        static Rect buttonRect;
        static bool enable = false;
        static UGSArButtonInToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/logo_expiria_ico.png");

            string name = "Expiria3D";

            GUI.backgroundColor = new Color(0f, 0.7f, 1, 1);
            if (GUILayout.Toggle(enable, new GUIContent(null, tex, name), new GUIStyle("Command")
            {
                fixedWidth = 90,
                padding = new RectOffset(0, 0, 0, 0)
                
            }, GUILayout.Width(90)))
            {
                ArWindow.ShowWindow();
                enable = false;
            }

            GUI.backgroundColor = Color.blue;

            if (Event.current.type == EventType.Repaint)
                buttonRect = GUILayoutUtility.GetLastRect();
        }
    }

    namespace ExpiriaToolbarExtender
    {
        public static class ToolbarCallback
        {
            static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            static Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
            static Type m_iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
            static PropertyInfo m_windowBackend = m_guiViewType.GetProperty("windowBackend",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            static PropertyInfo m_viewVisualTree = m_iWindowBackendType.GetProperty("visualTree",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            static FieldInfo m_imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            static ScriptableObject m_currentToolbar;

            /// <summary>
            /// Callback for toolbar OnGUI method.
            /// </summary>
            public static Action OnToolbarGUI;
            public static Action OnToolbarGUILeft;
            public static Action OnToolbarGUIRight;

            static ToolbarCallback()
            {
                EditorApplication.update -= OnUpdate;
                EditorApplication.update += OnUpdate;
            }

            static void OnUpdate()
            {
                // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
                if (m_currentToolbar == null)
                {
                    // Find toolbar
                    var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                    m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                    if (m_currentToolbar != null)
                    {
                        var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                        var rawRoot = root.GetValue(m_currentToolbar);
                        var mRoot = rawRoot as VisualElement;
                        RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                        RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                        void RegisterCallback(string rootX, Action cb)
                        {
                            var toolbarZone = mRoot.Q(rootX);

                            var parent = new VisualElement()
                            {
                                style = {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                                maxWidth = 100
                            }
                            };
                            var container = new IMGUIContainer();
                            container.onGUIHandler += () =>
                            {
                                cb?.Invoke();
                            };
                            parent.Add(container);
                            toolbarZone.Add(parent);
                        }
                    }
                }
            }

            static void OnGUI()
            {
                var handler = OnToolbarGUI;
                if (handler != null) handler();
            }
        }

        [InitializeOnLoad]
        public static class ToolbarExtender
        {
            static int m_toolCount;
            static GUIStyle m_commandStyle = null;

            public static readonly List<Action> LeftToolbarGUI = new List<Action>();
            public static readonly List<Action> RightToolbarGUI = new List<Action>();

            static ToolbarExtender()
            {
                Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

#if UNITY_2019_1_OR_NEWER
                string fieldName = "k_ToolCount";
#else
			string fieldName = "s_ShownToolIcons";
#endif

                FieldInfo toolIcons = toolbarType.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

#if UNITY_2019_3_OR_NEWER
                m_toolCount = toolIcons != null ? ((int)toolIcons.GetValue(null)) : 8;
#elif UNITY_2019_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 7;
#elif UNITY_2018_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 6;
#else
			m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 5;
#endif

                ToolbarCallback.OnToolbarGUI = OnGUI;
                ToolbarCallback.OnToolbarGUILeft = GUILeft;
                ToolbarCallback.OnToolbarGUIRight = GUIRight;
            }

#if UNITY_2019_3_OR_NEWER
            public const float space = 8;
#else
		public const float space = 10;
#endif
            public const float largeSpace = 20;
            public const float buttonWidth = 32;
            public const float dropdownWidth = 80;
#if UNITY_2019_1_OR_NEWER
            public const float playPauseStopWidth = 140;
#else
		public const float playPauseStopWidth = 100;
#endif

            static void OnGUI()
            {
                // Create two containers, left and right
                // Screen is whole toolbar

                if (m_commandStyle == null)
                {
                    m_commandStyle = new GUIStyle("CommandLeft");
                }

                var screenWidth = EditorGUIUtility.currentViewWidth;

                // Following calculations match code reflected from Toolbar.OldOnGUI()
                float playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2);

                Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
                leftRect.xMin += space; // Spacing left
                leftRect.xMin += buttonWidth * m_toolCount; // Tool buttons
#if UNITY_2019_3_OR_NEWER
                leftRect.xMin += space; // Spacing between tools and pivot
#else
			leftRect.xMin += largeSpace; // Spacing between tools and pivot
#endif
                leftRect.xMin += 64 * 2; // Pivot buttons
                leftRect.xMax = playButtonsPosition;

                Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
                rightRect.xMin = playButtonsPosition;
                rightRect.xMin += m_commandStyle.fixedWidth * 3; // Play buttons
                rightRect.xMax = screenWidth;
                rightRect.xMax -= space; // Spacing right
                rightRect.xMax -= dropdownWidth; // Layout
                rightRect.xMax -= space; // Spacing between layout and layers
                rightRect.xMax -= dropdownWidth; // Layers
#if UNITY_2019_3_OR_NEWER
                rightRect.xMax -= space; // Spacing between layers and account
#else
			rightRect.xMax -= largeSpace; // Spacing between layers and account
#endif
                rightRect.xMax -= dropdownWidth; // Account
                rightRect.xMax -= space; // Spacing between account and cloud
                rightRect.xMax -= buttonWidth; // Cloud
                rightRect.xMax -= space; // Spacing between cloud and collab
                rightRect.xMax -= 78; // Colab

                // Add spacing around existing controls
                leftRect.xMin += space;
                leftRect.xMax -= space;
                rightRect.xMin += space;
                rightRect.xMax -= space;

                // Add top and bottom margins
#if UNITY_2019_3_OR_NEWER
                leftRect.y = 4;
                leftRect.height = 22;
                rightRect.y = 4;
                rightRect.height = 22;
#else
			leftRect.y = 5;
			leftRect.height = 24;
			rightRect.y = 5;
			rightRect.height = 24;
#endif

                if (leftRect.width > 0)
                {
                    GUILayout.BeginArea(leftRect);
                    GUILayout.BeginHorizontal();
                    foreach (var handler in LeftToolbarGUI)
                    {
                        handler();
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }

                if (rightRect.width > 0)
                {
                    GUILayout.BeginArea(rightRect);
                    GUILayout.BeginHorizontal();
                    foreach (var handler in RightToolbarGUI)
                    {
                        handler();
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }

            //public static void InsetAtLeft(Action handler)
            //{
            //    bool exceptionFound = false;

            //    // Get all loaded assemblies
            //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //    // Find the type in all assemblies
            //    Type type = null;
            //    foreach (var assembly in assemblies)
            //    {
            //        type = assembly.GetTypes().FirstOrDefault(t => t.FullName == "UnityPandoraToolbarExtender.PandoraToolbarExtender");
            //        if (type != null)
            //        {
            //            break;
            //        }
            //    }

            //    if (type != null)
            //    {
            //        // If the class exists, get the LeftToolbarGUI property
            //        FieldInfo field = type.GetField("LeftToolbarGUI", BindingFlags.Static | BindingFlags.Public);
            //        if (field != null)
            //        {
            //            // Get the object that the property belongs to
            //            var list = field.GetValue(null) as List<Action>;

            //            // Get the Insert method of the object
            //            if (list != null)
            //            {
            //                list.Insert(list.Count-1, handler);
            //            }
            //            else
            //            {
            //                exceptionFound = true;
            //            }
            //        }
            //        else
            //        {
            //            exceptionFound = true;
            //        }
            //    }
            //    else
            //    {
            //        exceptionFound = true;
            //    }

            //    if (exceptionFound)
            //    {
            //        LeftToolbarGUI.Add(handler);
            //    }
            //}

            public static void GUILeft()
            {
                GUILayout.BeginHorizontal();
                foreach (var handler in LeftToolbarGUI)
                {
                    handler();
                }
                GUILayout.EndHorizontal();
            }

            public static void GUIRight()
            {
                GUILayout.BeginHorizontal();
                foreach (var handler in RightToolbarGUI)
                {
                    handler();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}