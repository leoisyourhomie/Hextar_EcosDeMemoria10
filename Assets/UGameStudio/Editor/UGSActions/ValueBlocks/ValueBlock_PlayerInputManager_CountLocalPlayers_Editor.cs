namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_PlayerInputManager_CountLocalPlayers_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override int Order => 0;

        public override bool IsThisBlockAssigned => block != null;

        public override ReactivePropertyInfo[] ReactiveProperties
        {
            get
            {
                if (block == null || !block.RequiredObject.IsReactiveOrAnchored)
                {
                    return new ReactivePropertyInfo[0];
                }
                else
                {
                    return new ReactivePropertyInfo[]
                    {
                        new ReactivePropertyInfo() { propertyNameSpanish = "Objeto", propertyNameEnglish = "Object" }
                    };
                }
            }
        }

        public override string NameInBuilderList_Spanish => "Count Local Players";

        public override string NameInBuilderList_English => "Count Local Players";

        public override string NameInEditor_Spanish => "Count Local Players";

        public override string NameInEditor_English => "Count Local Players";

        public override string DescriptionSpanish => "Count Local Players";

        public override string DescriptionEnglish => "Count Local Players";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInputManager();

        public override string IconName => "d_EventTrigger Icon";

        public string Text()
        {
            if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                return "Valor";
            else
                return "Value";
        }

        private ValueBlock_PlayerInputManager_CountLocalPlayers block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_PlayerInputManager_CountLocalPlayers)_valueBlock;

            //Draw the fields for the block
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
