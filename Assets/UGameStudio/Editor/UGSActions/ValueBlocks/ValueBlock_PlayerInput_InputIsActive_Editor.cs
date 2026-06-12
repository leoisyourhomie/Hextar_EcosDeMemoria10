namespace UGSSpace
{
    using System.Collections.Generic;
    using UGSSpace.UGameActions;
    public class ValueBlock_PlayerInput_InputIsActive_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => block != null;

        public override ReactivePropertyInfo[] ReactiveProperties
        {
            get
            {
                if(block == null || !block.RequiredObject.IsReactiveOrAnchored)
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

        public override string NameInBuilderList_Spanish => "Input Is Active";

        public override string NameInBuilderList_English => "Input Is Active";

        public override string NameInEditor_Spanish => "Input Is Active";

        public override string NameInEditor_English => "Input Is Active";

        public override string DescriptionSpanish => "Devuelve verdadero si el jugador tiene habilitados los Inputs. Para que el bloque reaccione de forma correcta, debes tener seleccionada la opción de Invoke C Sharp Events en el campo de Behavior del componente PlayerInput.";

        public override string DescriptionEnglish => "Returns true if the player has Inputs enabled. In order for the block to react correctly, you must have the Invoke C Sharp Events option selected in the Behavior field of the PlayerInput component.";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input_PlayerInput();

        public override string IconName => "d_EventTrigger Icon";

        private ValueBlock_PlayerInput_InputIsActive block;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            block ??= (ValueBlock_PlayerInput_InputIsActive)_valueBlock;
            ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => block.RequiredObject = selection, block.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow);
        }
    }
}
