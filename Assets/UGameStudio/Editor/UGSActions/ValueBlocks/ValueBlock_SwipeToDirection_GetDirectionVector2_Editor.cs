namespace UGSSpace
{
    using System.Collections.Generic;
    using UGameActions;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_SwipeToDirection_GetDirectionVector2_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;
        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "Swipe hacia dirección (Vector2)";

        public override string NameInBuilderList_English => "Swipe To Direction (Vector2)";

        public override string NameInEditor_Spanish => "Swipe hacia";

        public override string NameInEditor_English => "Swipe To";

        public override string DescriptionSpanish => "Comprueba si se desliza hacia una dirección en especifico";

        public override string DescriptionEnglish => "Checks if it slides in a specific direction";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_Swipe();

        public override string IconName => "d_AvatarPivot@2x";

        public string Text()
        {
                if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                    return "Valor";
                else
                    return "Value";
        }

        private ValueBlock_Input_SwipeToDirection_GetDirectionVector2 block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_Input_SwipeToDirection_GetDirectionVector2)_valueBlock;

            if (block.InputActionForPosition.name == "NULL_NAME")
            {
                block.InputActionForPosition = new InputAction("Mouse Position", InputActionType.Value);
                block.InputActionForPosition.AddBinding("<Mouse>/position");
                block.InputActionForPosition.AddBinding("<Touchscreen>/position");

            }

            if (block.InputActionForPress.name == "NULL_NAME")
            {
                block.InputActionForPress = new InputAction("Mouse Button Press", InputActionType.Button);
                block.InputActionForPress.AddBinding("<Mouse>/leftButton");
                block.InputActionForPress.AddBinding("<Touchscreen>/Press");
            }

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                Rect rect = EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Directions", EditorStyles.popup))
                {
                    if (!FlowMapEditor.CanOpenPopup())
                    {
                        GUIUtility.ExitGUI();
                    }

                    PopupWindow.Show(rect, new SwipeDirectionsSelectorWindow(block.SelectableDirections, (x) =>
                    {
                        FlowEditor.ForceDirtyAndUndo();
                        block.SelectableDirections = x;
                    }));
                }

                if (GUILayout.Button(new GUIContent(block.InputActionForPosition.name, EditorGUIUtility.IconContent("d_StandaloneInputModule Icon").image), EditorStyles.popup, GUILayout.Height(22), GUILayout.Width(140)))
                {
                    if (!FlowMapEditor.CanOpenPopup())
                    {
                        GUIUtility.ExitGUI();
                    }

                    PopupWindow.Show(rect, new CustomInputPopup(block.InputActionForPosition, (x) =>
                    {
                        FlowEditor.ForceDirtyAndUndo();
                        block.InputActionForPosition = x;
                    }, block.ValueType));
                }

                if (GUILayout.Button(new GUIContent(block.InputActionForPress.name, EditorGUIUtility.IconContent("d_StandaloneInputModule Icon").image), EditorStyles.popup, GUILayout.Height(22), GUILayout.Width(145)))
                {
                    if (!FlowMapEditor.CanOpenPopup())
                    {
                        GUIUtility.ExitGUI();
                    }

                    PopupWindow.Show(rect, new CustomInputPopup(block.InputActionForPress, (x) =>
                    {
                        FlowEditor.ForceDirtyAndUndo();
                        block.InputActionForPress = x;
                    }, block.ValueType));
                }

                EditorGUIUtility.labelWidth = 65;
                string distanceDescription;
                string distanceTxt;

                if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                {
                    distanceDescription = "Representa el porcentaje recorrido en la pantalla.";
                    distanceTxt = "Distancia:";
                }
                else
                {
                    distanceDescription = "Represents the percentage traveled on the screen.";
                    distanceTxt = "Distance:";
                }

                float currentDistance = block.SwipeDistance * 100;
                currentDistance = EditorGUILayout.FloatField(new GUIContent(distanceTxt, distanceDescription), currentDistance, GUILayout.Width(110));
                if (currentDistance <= 0)
                {
                    block.SwipeDistance = 0;
                }
                else if (currentDistance >= 100)
                {
                    block.SwipeDistance = 1;
                }
                else
                {
                    block.SwipeDistance = currentDistance / 100;
                }
                EditorGUILayout.LabelField("%", GUILayout.Width(15));

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
