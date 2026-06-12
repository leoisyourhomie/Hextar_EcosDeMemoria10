namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using UGameActions;
    
    public class ValueBlock_Input_CustomInputIsPressed_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;
        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "Custom Input Is Pressed";

        public override string NameInBuilderList_English => "Custom Input Is Pressed";

        public override string NameInEditor_Spanish => "Input Personalizado Es Presionado";

        public override string NameInEditor_English => "Custom Input Is Pressed";

        public override string DescriptionSpanish => "Custom Input Is Pressed";

        public override string DescriptionEnglish => "Custom Input Is Pressed";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_CustomInput();

        public override string IconName => "d_EventTrigger Icon";


        ValueBlock_Input_CustomInputIsPressed block;
        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                block ??= (ValueBlock_Input_CustomInputIsPressed)_valueBlock;

                Rect rect = EditorGUILayout.BeginHorizontal();

                string btnText = block.CustomAction == null ? "Select Input" : block.CustomAction.name;

                if (GUILayout.Button(new GUIContent(btnText, EditorGUIUtility.IconContent("d_StandaloneInputModule Icon").image), EditorStyles.popup, GUILayout.Height(22), GUILayout.Width(140)))
                {
                    if (!FlowMapEditor.CanOpenPopup())
                    {
                        GUIUtility.ExitGUI();
                    }

                    PopupWindow.Show(rect, new CustomInputPopup(block.CustomAction, (x) =>
                    {
                        FlowEditor.ForceDirtyAndUndo();
                        block.CustomAction = x;
                    }, block.ValueType));
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
