namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;

        public class UGSAction_PlayerInputManager_DisableJoining_Editor : UGSAction_Editor
        {
            UGSAction_PlayerInputManager_DisableJoining thisAction;

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

            public override string NameInBuilderList_Spanish => "No Permitir Unir Nuevos Jugadores";

            public override string NameInBuilderList_English => "Do Not Allow New Players To Join";

            public override string NameInEditor_Spanish => "No Permitir Unir Nuevos Jugadores";

            public override string NameInEditor_English => "Do Not Allow New Players to Join";

            public override string DescriptionSpanish => "No Permitir Unir Nuevos Jugadores";

            public override string DescriptionEnglish => "Do Not Allow New Players To Join";
            
            public override string IconName => "d_EventTrigger Icon";

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_DisableJoining)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_DisableJoining)_action;

                //Draw the fields here
                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => thisAction.RequiredObject = selection, thisAction.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow, "Player Input Manager: ");
            }
        }
    }
}
