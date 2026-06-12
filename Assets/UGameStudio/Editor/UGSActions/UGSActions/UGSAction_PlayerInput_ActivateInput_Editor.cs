namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;

        public class UGSAction_PlayerInput_ActivateInput_Editor : UGSAction_Editor
        {
            UGSAction_PlayerInput_ActivateInput thisAction;
            public override bool IsThisActionAssigned => thisAction != null;
            public override ReactivePropertyInfo[] ReactiveProperties
            {
                get
                {
                    if (thisAction == null || !thisAction.RequiredObject.IsReactiveOrAnchored)
                        return new ReactivePropertyInfo[0];

                    ReactivePropertyInfo objectToEnableInfo = new ReactivePropertyInfo
                    {
                        propertyNameSpanish = "PlayerInput",
                        propertyNameEnglish = "PlayerInput"
                    };

                    return new ReactivePropertyInfo[] { objectToEnableInfo };
                }
            }

            public override int Order => 0;

            public override FlowBuilderCategory Category => new UGSActionCategory_Inputs_PlayerInput();

            public override string NameInBuilderList_Spanish => "Activar Inputs En Jugador";

            public override string NameInBuilderList_English => "Activate Inputs In Player";

            public override string NameInEditor_Spanish => "Activar Inputs En Jugador";

            public override string NameInEditor_English => "Activate Inputs In Player";

            public override string DescriptionSpanish => "Activar Inputs En Jugador";

            public override string DescriptionEnglish => "Activate Inputs In Player";
            
            public override string IconName => "d_EventTrigger Icon";

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInput_ActivateInput)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInput_ActivateInput)_action;

                //Draw the fields here
                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => thisAction.RequiredObject = selection, thisAction.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow, "Player Input: ");
            }
        }
    }
}
