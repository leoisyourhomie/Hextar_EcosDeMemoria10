namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class CustomInputPopup : PopupWindowContent
    {
        System.Action<InputAction> onInputEdited;
        Vector2 scrollPos;
        private InputPath inputPath;

        SerializedProperty inputProperty;
        SerializedObject serializedBlock;
        ValueTypeEnum typeAccepted;

        public static Texture GetIconFromItem(CostumeItem item)
        {
            Texture2D textureToReturn = AssetPreview.GetAssetPreview(item.gameObjectOfItem);

            if (textureToReturn == null)
                textureToReturn = AssetPreview.GetMiniThumbnail(item.gameObjectOfItem);

            if (textureToReturn == null)
                textureToReturn = EditorGUIUtility.IconContent("SceneViewVisibility@2x").image as Texture2D;

            return textureToReturn;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(225, 290);
        }

        public CustomInputPopup(InputAction currentInput, System.Action<InputAction> onInputActionEdited, ValueTypeEnum typeAccepted)
        {
            this.onInputEdited = onInputActionEdited;
            this.typeAccepted = typeAccepted;
            // Initialize inputPath only once
            if (inputPath == null)
            {
                inputPath = ScriptableObject.CreateInstance<InputPath>();
            }

            inputPath.actions = currentInput;

            serializedBlock ??= new SerializedObject(inputPath);
            inputProperty ??= serializedBlock.FindProperty("actions");
        }

        public override void OnClose()
        {
            ApplyChanges();
        }

        void ApplyChanges()
        {
            serializedBlock.ApplyModifiedProperties();
            onInputEdited?.Invoke(inputPath.actions);
        }

        public override void OnGUI(Rect rect)
        {
            //Variables for scroll
            float width = this.editorWindow.position.width;
            float height = this.editorWindow.position.height;
            //Start scroll view

            float spacing = 87;
            if (typeAccepted == ValueTypeEnum.Trigger)
            {
                spacing = 22;
            }

            string nameText = (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish) ? "Nombre: " : "Name: ";

            EditorGUIUtility.labelWidth = 54;
            string currText = inputPath.actions.name;
            currText = EditorGUILayout.TextField(nameText, inputPath.actions.name);
            if(string.IsNullOrEmpty(currText))
            {
                currText = " ";
            }
            if (currText != inputPath.actions.name)
            {
                inputPath.actions.Rename(currText);
            }
            EditorGUIUtility.labelWidth = 0;

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(width - 1), GUILayout.Height(height - spacing));

            serializedBlock.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(inputProperty, GUILayout.Width(205));

            if (EditorGUI.EndChangeCheck())
            {
                ApplyChanges();
            }

            GUILayout.EndScrollView();

            if (typeAccepted != ValueTypeEnum.Trigger)
            {
                if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.English)
                {
                    EditorGUILayout.LabelField("This value block <b>will return a value of type " + typeAccepted.ToString() + "</b>. If any of the inputs selected has a different type, the value will be ignored.", new GUIStyle(EditorStyles.wordWrappedLabel) { richText = true });
                }
                else
                {
                    EditorGUILayout.LabelField("Este bloque de valor <b>devolverá un valor de tipo " + typeAccepted.ToString() + "</b>. Si alguno de los inputs seleccionados tiene un tipo diferente, el valor será ignorado.", new GUIStyle(EditorStyles.wordWrappedLabel) { richText = true });
                }
            }
        }
    }

    [System.Serializable]
    public class InputPath : ScriptableObject
    {
        public InputAction actions = new InputAction();
    }
}

