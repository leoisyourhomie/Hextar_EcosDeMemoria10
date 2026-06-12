namespace UGSSpace
{
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_PlayerInputManager_JoiningIsEnabled_Editor : FunctionsEditor.ValueBlockEditor
    {
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

        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "La Unión De Jugadores Está Habilitada";

        public override string NameInBuilderList_English => "Player Joining Is Enabled";

        public override string NameInEditor_Spanish => "La Unión De Jugadores Está Habilitada";

        public override string NameInEditor_English => "Player Joining Is Enabled";

        public override string DescriptionSpanish => "La Unión De Jugadores Está Habilitada";

        public override string DescriptionEnglish => "Player Joining Is Enabled";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInputManager();

        public override string IconName => "d_EventTrigger Icon";

        private ValueBlock_PlayerInputManager_JoiningIsEnabled block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_PlayerInputManager_JoiningIsEnabled)_valueBlock;
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
