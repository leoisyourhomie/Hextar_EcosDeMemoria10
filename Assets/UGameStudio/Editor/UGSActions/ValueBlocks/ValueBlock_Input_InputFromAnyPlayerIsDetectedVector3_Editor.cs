namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using UGameActions;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_InputFromAnyPlayerIsDetectedVector3_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;
        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "Input From Any Player Is Detected (Vector3)";

        public override string NameInBuilderList_English => "Input From Any Player Is Detected (Vector3)";

        public override string NameInEditor_Spanish => "Input Desde Cualquier Jugador";

        public override string NameInEditor_English => "Input From Any Player";

        public override string DescriptionSpanish => "Input From Any Player Is Detected (Vector3)";

        public override string DescriptionEnglish => "Input From Any Player Is Detected (Vector3)";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_InputFromAnyPlayer();

        public override string IconName => "d_EventTrigger Icon";

        ValueBlock_Input_InputFromAnyPlayerIsDetectedVector3 block;
        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                block ??= (ValueBlock_Input_InputFromAnyPlayerIsDetectedVector3)_valueBlock;

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 42;

                block.InputActionRef = EditorGUILayout.ObjectField("Input: ", block.InputActionRef, typeof(InputActionReference), true, GUILayout.Width(160)) as InputActionReference;

                block.ReactionType = (InputReactionTypeEnum)EditorGUILayout.EnumPopup("React: ", block.ReactionType, GUILayout.Width(160));

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
