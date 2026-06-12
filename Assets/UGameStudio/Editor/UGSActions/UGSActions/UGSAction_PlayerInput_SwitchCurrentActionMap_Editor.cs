namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;
        using UnityEngine;

        public class UGSAction_PlayerInput_SwitchCurrentActionMap_Editor : UGSAction_Editor
        {
            public override bool IsThisActionAssigned => thisAction != null;
            public override ReactivePropertyInfo[] ReactiveProperties
            {
                get
                {
                    if (thisAction == null)
                        return new ReactivePropertyInfo[0];

                    List<ReactivePropertyInfo> reactiveProperties = new List<ReactivePropertyInfo>();

                    if (thisAction.RequiredObject.IsReactiveOrAnchored)
                    {
                        reactiveProperties.Add(new ReactivePropertyInfo
                        {
                            propertyNameSpanish = "PlayerInput",
                            propertyNameEnglish = "PlayerInput"
                        });
                    }

                    if (thisAction.RequiredValue.IsReactiveOrAnchored)
                    {
                        reactiveProperties.Add(new ReactivePropertyInfo
                        {
                            propertyNameSpanish = "Action Map",
                            propertyNameEnglish = "Action Map"
                        });
                    }   

                    return reactiveProperties.ToArray();
                }
            }

            public override int Order => 0;

            public override FlowBuilderCategory Category => new UGSActionCategory_Inputs_PlayerInput();

            public override string NameInBuilderList_Spanish => "Cambiar El Action Map Actual";

            public override string NameInBuilderList_English => "Switch Current Action Map";

            public override string NameInEditor_Spanish => "Cambiar El Action Map Actual";

            public override string NameInEditor_English => "Switch Current Action Map";

            public override string DescriptionSpanish => "Cambiar El Action Map Actual";

            public override string DescriptionEnglish => "Switch Current Action Map";
            
            public override string IconName => "d_EventTrigger Icon";

            UGSAction_PlayerInput_SwitchCurrentActionMap thisAction;

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInput_SwitchCurrentActionMap)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInput_SwitchCurrentActionMap)_action;

                //Draw the fields here
                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => thisAction.RequiredObject = selection, thisAction.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow, "Player Input: ");
                GUILayout.Space(10);
                FunctionsEditor.DrawValueBlockField(ValueTypeEnum.Text, flowEditor, actionsReactivity, thisAction.RequiredValue, disableSelectionForSpecificObjectsInScene, receivedLocalVariables, isThisFieldInsideValueSelectorWindow, (x) => thisAction.RequiredValue = x, "Action Map: ");
            }
        }
    }
}
