namespace UGSSpace
{
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_Input_CurrentActionMapName_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;
        public override int Order => 0;

        public override string NameInBuilderList_Spanish => "Current Action Map Name";

        public override string NameInBuilderList_English => "Current Action Map Name";

        public override string NameInEditor_Spanish => "Current Action Map Name";

        public override string NameInEditor_English => "Current Action Map Name";

        public override string DescriptionSpanish => "Devuelve el nombre del Action Map actual. Para que el bloque reaccione de forma correcta, debes tener seleccionada la opción de Invoke C Sharp Events en el campo de Behavior del componente PlayerInput.";

        public override string DescriptionEnglish => "Returns the name of the current Action Map. In order for the block to react correctly, you must have the Invoke C Sharp Events option selected in the Behavior field of the PlayerInput component.";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInput();

        public override string IconName => "d_EventTrigger Icon";

        private ValueBlock_Input_CurrentActionMapName block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_Input_CurrentActionMapName)_valueBlock;
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
