namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;

        public class UGSAction_PlayerInputManager_EnableJoining_Editor : UGSAction_Editor
        {
            public override bool IsThisActionAssigned => thisAction != null;
            public override ReactivePropertyInfo[] ReactiveProperties
            {
                get
                {
                    if (thisAction == null || !thisAction.RequiredObject.IsReactiveOrAnchored)
                        return new ReactivePropertyInfo[0];

                    ReactivePropertyInfo objectToEnableInfo = new ReactivePropertyInfo
                    {
                        propertyNameSpanish = "Player Input Manager",
                        propertyNameEnglish = "Player Input Manager"
                    };

                    return new ReactivePropertyInfo[] { objectToEnableInfo };
                }
            }

            public override int Order => 0;

            public override FlowBuilderCategory Category => new UGSActionCategory_Inputs_PlayerInputManager();

            public override string NameInBuilderList_Spanish => "Permitir Unir Nuevos Jugadores";

            public override string NameInBuilderList_English => "Allow Join New Players";

            public override string NameInEditor_Spanish => "Permitir Unir Nuevos Jugadores";

            public override string NameInEditor_English => "Allow Join New Players";

            public override string DescriptionSpanish => "Permitir Unir Nuevos Jugadores";

            public override string DescriptionEnglish => "Allow Join New Players";
            
            public override string IconName => "d_EventTrigger Icon";

            UGSAction_PlayerInputManager_EnableJoining thisAction;
            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_EnableJoining)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_EnableJoining)_action;

                //Draw the fields here
                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => thisAction.RequiredObject = selection, thisAction.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow, "Player Input Manager: ");
            }
        }
    }
}
