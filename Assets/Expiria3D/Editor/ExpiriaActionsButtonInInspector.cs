namespace Expiria3DSpace
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System;

    [InitializeOnLoad]
    public class ExpiriaActionsButtonInInspector : Editor
    {
        private const double RefreshRate = 0.1f;

        private static double _previousUpdateTime;

        private static bool IsButtonVisible => Selection.activeGameObject != null;

        static ExpiriaActionsButtonInInspector()
        {
            EditorApplication.update += UpdateEditor;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            bool visibility = IsButtonVisible;

            // Actualiza la visibilidad de los botones personalizados
            EditorWindow[] editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in editorWindows)
            {
                if (window.GetType().Name != "InspectorWindow") continue;

                VisualElement addComponentButton = window.rootVisualElement.Q(className: "unity-inspector-add-component-button");
                if (addComponentButton == null) continue;

                VisualElement parent = addComponentButton.parent;

                // Busca el contenedor de botones personalizado
                VisualElement buttonContainer = parent.Q("Expiria3DButtonContainer");
                if (buttonContainer == null) continue;

                foreach (var button in buttonContainer.Children())
                {
                    button.visible = visibility;
                }
            }
        }

        private static void UpdateEditor()
        {
            if (EditorApplication.timeSinceStartup - _previousUpdateTime < RefreshRate) return;
            _previousUpdateTime = EditorApplication.timeSinceStartup;

            EditorWindow[] editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in editorWindows)
            {
                if (window.GetType().Name != "InspectorWindow") continue;

                VisualElement addComponentButton = window.rootVisualElement.Q(className: "unity-inspector-add-component-button");
                if (addComponentButton == null) continue;

                VisualElement parent = addComponentButton.parent;

                // Verifica si el contenedor ya existe
                VisualElement existingContainer = parent.Q("Expiria3DButtonContainer");
                if (existingContainer != null) continue;

                VisualElement buttonContainer = CreateButtonContainer();
                parent.Add(buttonContainer);

                Button customButtonAR = CreateCustomButtonAR();
                buttonContainer.Add(customButtonAR);

            }
        }

        private static VisualElement CreateButtonContainer()
        {
            return new VisualElement
            {
                name = "Expiria3DButtonContainer", // Asigna un nombre único al contenedor
                style = {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.Center,
                    marginTop = -10,
                    alignItems = Align.Center
                }
            };
        }

        private static Button CreateCustomButtonAR()
        {
            string buttonName;

            buttonName = " Acciones de Expiria3D";

            Button button = new Button(ButtonClickAR);
            button.style.flexDirection = FlexDirection.Row;
            button.style.width = 190;
            button.style.height = 25;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;

            if(AssetDatabase.IsValidFolder("Assets/UGameStudio"))
                button.style.marginTop = 12;

            button.style.alignItems = Align.Center; // Center the content vertically
            button.style.justifyContent = Justify.Center; // Center the content horizontally

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Expiria3D/EditorIcons/ArActions/ar_click.png");

            // Add the icon to the button.
            VisualElement iconElement = new VisualElement();
            iconElement.style.backgroundImage = icon;
            iconElement.style.width = 18;
            iconElement.style.height = 20;

            button.Add(iconElement);

            // Add the text to the button.
            Label label = new Label(buttonName);

            button.Add(label);
            button.visible = IsButtonVisible;

            return button;
        }

        private static void ButtonClickAR()
        {
            Vector2 mousePos = Event.current.mousePosition;
            Rect r = new Rect(mousePos.x, mousePos.y, 0, 0);
            UnityEditor.PopupWindow.Show(r, new ARActionsPopup());
        }

        public class ARActionsPopup : PopupWindowContent
        {
            public override Vector2 GetWindowSize()
            {
                return new Vector2(400, 400);
            }

            Vector2 scrollPos;

            public override void OnGUI(Rect rect)
            {
                GUIStyle styleTxt = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 15, wordWrap = true };

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Selecciona una acción para agregar al objeto", new GUIStyle(styleTxt) { fontStyle = FontStyle.Bold });

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                foreach (var actionInfo in ArActionsEditorTools.actionsInfo)
                {
                    //------------------------------ START open url ---------------------------------
                    Rect btnRect = EditorGUILayout.BeginHorizontal(ArActionsEditorTools.BigButtonStyle);
                    {
                        //draw icon
                        GUILayout.Label(ArActionsEditorTools.GetIcon(actionInfo.Item1), GUILayout.Width(30), GUILayout.Height(30));

                        //flexible vertical
                        EditorGUILayout.BeginVertical(GUILayout.Height(30));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(actionInfo.Item2, styleTxt);
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        ArAction arAction = Selection.activeGameObject.AddComponent<ArAction>();
                        arAction.actionData = (ArActionBase)Activator.CreateInstance(actionInfo.Item1);
                        this.editorWindow.Close();
                    }
                }
                //----------------------------- END open url ---------------------------------

                GUILayout.Space(10);
                EditorGUILayout.EndScrollView();
            }
        }

    }
}
