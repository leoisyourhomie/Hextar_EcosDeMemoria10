namespace UGSSpace
{
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_PlayerInput_LocalPlayerIndex_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override int Order => 0;

        public override bool IsThisBlockAssigned => block != null;
        public override ReactivePropertyInfo[] ReactiveProperties
        {
            get
            {
                if (block == null)
                    return new ReactivePropertyInfo[0];

                List<ReactivePropertyInfo> consoleMessageInfo = new List<ReactivePropertyInfo>();

                if (block.RequiredObject.IsReactiveOrAnchored)
                    consoleMessageInfo.Add(new ReactivePropertyInfo() { propertyNameSpanish = "PlayerInput", propertyNameEnglish = "PlayerInput" });

                return consoleMessageInfo.ToArray();
            }
        }

        public override string NameInBuilderList_Spanish => "Local Player Index";

        public override string NameInBuilderList_English => "Local Player Index";

        public override string NameInEditor_Spanish => "Local Player Index";

        public override string NameInEditor_English => "Local Player Index";

        public override string DescriptionSpanish => "Local Player Index";

        public override string DescriptionEnglish => "Local Player Index";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInput();

        public override string IconName => "d_EventTrigger Icon";

        private ValueBlock_PlayerInput_LocalPlayerIndex block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_PlayerInput_LocalPlayerIndex)_valueBlock;

            //Draw the fields for the block
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
