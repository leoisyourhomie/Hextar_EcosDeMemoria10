namespace UGSSpace
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UGSSpace.UGameActions;
    public class ValueBlock_Input_InputFromAnyPlayerIsPressed_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override int Order => 0;
        public override bool IsThisBlockAssigned => block != null;
        public override string NameInBuilderList_Spanish => "Input From Any Player Is Pressed";

        public override string NameInBuilderList_English => "Input From Any Player Is Pressed";

        public override string NameInEditor_Spanish => "Input Desde Cualquier Jugador Es Presionado";

        public override string NameInEditor_English => "Input From Any Player";

        public override string DescriptionSpanish => "Input From Any Player Is Pressed";

        public override string DescriptionEnglish => "Input From Any Player Is Pressed";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_InputFromAnyPlayer();

        public override string IconName => "d_EventTrigger Icon";

        ValueBlock_Input_InputFromAnyPlayerIsPressed block;
        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                block ??= (ValueBlock_Input_InputFromAnyPlayerIsPressed)_valueBlock;

                EditorGUIUtility.labelWidth = 42;

                block.InputActionRef = EditorGUILayout.ObjectField("Input: ", block.InputActionRef, typeof(InputActionReference), true, GUILayout.Width(160)) as InputActionReference;

                EditorGUIUtility.labelWidth = 0;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
