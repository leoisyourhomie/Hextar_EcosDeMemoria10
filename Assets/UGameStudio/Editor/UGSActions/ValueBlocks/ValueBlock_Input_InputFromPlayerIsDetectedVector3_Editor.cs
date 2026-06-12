namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using UGameActions;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_InputFromPlayerIsDetectedVector3_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;

        public override ReactivePropertyInfo[] ReactiveProperties
        {
            get
            {
                if (block == null || !block.RequiredPlayerInput.IsReactiveOrAnchored)
                {
                    return new ReactivePropertyInfo[0];
                }
                else
                {
                    return new ReactivePropertyInfo[]
                    {
                        new ReactivePropertyInfo() { propertyNameSpanish = "PlayerInput", propertyNameEnglish = "PlayerInput" }
                    };
                }
            }
        }

        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "Input From Player Is Detected (Vector3)";

        public override string NameInBuilderList_English => "Input From Player Is Detected (Vector3)";

        public override string NameInEditor_Spanish => "Input Desde Jugador";

        public override string NameInEditor_English => "Input From Player";

        public override string DescriptionSpanish => "Input From Player Is Detected (Vector3)";

        public override string DescriptionEnglish => "Input From Player Is Detected (Vector3)";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_InputFromPlayer();

        public override string IconName => "d_EventTrigger Icon";

        ValueBlock_Input_InputFromPlayerIsDetectedVector3 block;
        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_Input_InputFromPlayerIsDetectedVector3)_valueBlock;

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {

                EditorGUILayout.BeginHorizontal();

                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (x) => block.RequiredPlayerInput = x, block.RequiredPlayerInput
                    , receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);

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
