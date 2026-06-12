namespace UGSSpace
{
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_Input_DefaultActionMapName_Editor : FunctionsEditor.ValueBlockEditor
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

        public override string NameInBuilderList_Spanish => "Default Action Map Name";

        public override string NameInBuilderList_English => "Default Action Map Name";

        public override string NameInEditor_Spanish => "Default Action Map Name";

        public override string NameInEditor_English => "Default Action Map Name";

        public override string DescriptionSpanish => "Devuelve el nombre del Action Map por defecto.";

        public override string DescriptionEnglish => "Returns the name of the default Action Map.";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInput();

        public override string IconName => "d_EventTrigger Icon";

        private ValueBlock_Input_DefaultActionMapName block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_Input_DefaultActionMapName)_valueBlock;
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
