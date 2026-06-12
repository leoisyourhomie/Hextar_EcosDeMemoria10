namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;
        using UnityEditor;
        using UnityEngine;
        using UnityEngine.InputSystem;

        public class UGSAction_PlayerInputManager_SetPlayerPrefab_Editor : UGSAction_Editor
        {
            UGSAction_PlayerInputManager_SetPlayerPrefab thisAction;
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

            public override string NameInBuilderList_Spanish => "Asignar \"Player Prefab\" Al PlayerInputManager";

            public override string NameInBuilderList_English => "Set \"Player Prefab\" To PlayerInputManager";

            public override string NameInEditor_Spanish => "Asignar \"Player Prefab\" Al PlayerInputManager";

            public override string NameInEditor_English => "Set \"Player Prefab\" To PlayerInputManager";

            public override string DescriptionSpanish => "Asignar \"Player Prefab\" Al PlayerInputManager";

            public override string DescriptionEnglish => "Set \"Player Prefab\" To PlayerInputManager";

            public override string IconName => "d_EventTrigger Icon";

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_SetPlayerPrefab)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                if (thisAction == null)
                    thisAction = (UGSAction_PlayerInputManager_SetPlayerPrefab)_action;

                //Draw the fields here
                ObjectSelectorEditor.DrawField(flowEditor, actionsReactivity, (selection) => thisAction.RequiredObject = selection, thisAction.RequiredObject, receivedLocalVariables, disableSelectionForSpecificObjectsInScene, isThisFieldInsideValueSelectorWindow, "Player Input Manager: ");
                GUILayout.Space(5);
                EditorGUIUtility.labelWidth = 110;
                thisAction.PlayerPrefab = EditorGUILayout.ObjectField("Player Prefab: ", thisAction.PlayerPrefab, typeof(GameObject), false) as GameObject;

                if (thisAction.PlayerPrefab != null
                    && thisAction.PlayerPrefab.gameObject.GetComponent<PlayerInput>() == null)
                {
                    if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                    {
                        EditorGUILayout.HelpBox("El Player Prefab debe tener el componente de PlayerInput.", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("The Player Prefab must have the PlayerInput component.", MessageType.Error);
                    }
                }

                EditorGUIUtility.labelWidth = 0;
            }
        }
    }
}
