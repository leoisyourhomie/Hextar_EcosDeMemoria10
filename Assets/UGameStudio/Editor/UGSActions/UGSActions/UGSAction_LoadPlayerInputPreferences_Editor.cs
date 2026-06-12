namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;
        using UnityEngine;
        using UnityEditor;
        using UnityEngine.InputSystem;

        public class UGSAction_LoadPlayerInputPreferences_Editor : UGSAction_Editor
        {
            public override bool IsThisActionAssigned => thisAction != null;
            public override ReactivePropertyInfo[] ReactiveProperties
            {
                get
                {
                    return new ReactivePropertyInfo[0];
                }
            }
            public override int Order => 0;

            public override FlowBuilderCategory Category => new UGSActionCategory_InputPreferences();

            public override string NameInBuilderList_Spanish => "Cargar Preferencias De Inputs Del Jugador";

            public override string NameInBuilderList_English => "Load Player Input Preferences";

            public override string NameInEditor_Spanish => "Cargar Preferencias De Inputs Del Jugador";

            public override string NameInEditor_English => "Load Player Input Preferences";

            public override string DescriptionSpanish => "Cargar Preferencias De Inputs Del Jugador";

            public override string DescriptionEnglish => "Load Player Input Preferences";

            public override string IconName => "d_EventTrigger Icon";

            UGSAction_LoadPlayerInputPreferences thisAction;

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                thisAction ??= (UGSAction_LoadPlayerInputPreferences)_action;
                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                thisAction ??= (UGSAction_LoadPlayerInputPreferences)_action;

                if (UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                {
                    EditorGUILayout.LabelField("Define todas las acciones de input que quieres cargar", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("Define all the Input Actions that you want to load", EditorStyles.boldLabel);
                }

                GUILayout.Space(5);

                EditorGUIUtility.labelWidth = 100;
                for (int i = 0; i < thisAction.InputActions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    thisAction.InputActions[i] = (InputActionReference)EditorGUILayout.ObjectField("Input Action: ", thisAction.InputActions[i], typeof(InputActionReference), false);

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        thisAction.InputActions.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(5);

                if (GUILayout.Button("Add Input Action"))
                {
                    thisAction.InputActions.Add(null);
                }
            }
        }
    }
}
