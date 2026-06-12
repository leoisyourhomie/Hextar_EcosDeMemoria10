namespace UGSSpace
{
    namespace UGameActions
    {
        using System.Collections.Generic;
        using System.Linq;
        using UnityEditor;
        using UnityEngine;
        using UnityEngine.InputSystem;

        public class UGSAction_SetPlayerInputPreferences_Editor : UGSAction_Editor
        {
            public override bool IsThisActionAssigned => thisAction != null;
            public override ReactivePropertyInfo[] ReactiveProperties
            {
                get
                {
                    if (thisAction == null)
                        return new ReactivePropertyInfo[0];

                    return new ReactivePropertyInfo[0];
                }
            }

            public override int Order => 0;

            public override FlowBuilderCategory Category => new UGSActionCategory_InputPreferences();

            public override string NameInBuilderList_Spanish => "Modificar Preferencias De Inputs Del Jugador";

            public override string NameInBuilderList_English => "Change Player Input Preferences";

            public override string NameInEditor_Spanish => "Modificar Preferencias De Inputs Del Jugador";

            public override string NameInEditor_English => "Change Player Input Preferences";

            public override string DescriptionSpanish => "Modificar Preferencias De Inputs Del Jugador";

            public override string DescriptionEnglish => "Change Player Input Preferences";

            public override string IconName => "d_EventTrigger Icon";

            private GUIContent[] m_BindingOptions = new GUIContent[0];
            private string[] m_BindingOptionValues = new string[0];
            private int m_SelectedBindingOption = -1;

            UGSAction_SetPlayerInputPreferences thisAction;

            public override string Title(UGameActions.FlowEditor flowEditor, UGSAction _action, List<IUGameVariable> receivedLocalVariables)
            {
                thisAction ??= (UGSAction_SetPlayerInputPreferences)_action;

                return NameInEditor;
            }

            public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, UGSAction _action, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
            {
                thisAction ??= (UGSAction_SetPlayerInputPreferences)_action;
                EditorGUIUtility.labelWidth = 180f;
                EditorGUILayout.BeginVertical("groupbox");
                {
                    EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel, GUILayout.Width(200));
                    GUILayout.Space(4);
                    thisAction.UseTextMeshPro = EditorGUILayout.Toggle("Use Text Mesh Pro: ", thisAction.UseTextMeshPro);
                    thisAction.AllowDuplicates = EditorGUILayout.Toggle("Allow Duplicate Inputs: ", thisAction.AllowDuplicates);
                    GUILayout.Space(5);
                    thisAction.Panel = EditorGUILayout.ObjectField("Panel: ", thisAction.Panel, typeof(GameObject), true) as GameObject;

                    if (thisAction.UseTextMeshPro)
                    {
                        thisAction.WaitingForInput_TextInPanel = EditorGUILayout.ObjectField("Waiting For Input Text: ", thisAction.WaitingForInput_TextInPanel, typeof(TMPro.TextMeshProUGUI), true) as TMPro.TextMeshProUGUI;
                    }
                    else
                    {
                        thisAction.WaitingForInput_TextInPanel = EditorGUILayout.ObjectField("Waiting For Input Text: ", thisAction.WaitingForInput_TextInPanel, typeof(UnityEngine.UI.Text), true) as UnityEngine.UI.Text;
                    }

                    EditorGUILayout.BeginVertical("groupbox");
                    {
                        EditorGUILayout.LabelField("Controls to cancel operation", EditorStyles.boldLabel, GUILayout.Width(200));
                        GUILayout.Space(4);

                        EditorGUIUtility.labelWidth = 60f;

                        for (int i = 0; i < thisAction.ControlsToCancelOperation.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            thisAction.ControlsToCancelOperation[i] = EditorGUILayout.TextField("Control: ", thisAction.ControlsToCancelOperation[i]);

                            if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), new GUIStyle(EditorStyles.iconButton) { margin = new RectOffset(0, 0, 2, 0) }, GUILayout.MaxWidth(25f), GUILayout.MaxHeight(25f)))
                            {
                                thisAction.ControlsToCancelOperation.RemoveAt(i);
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (GUILayout.Button("Add Control"))
                        {
                            thisAction.ControlsToCancelOperation.Add("<Keyboard>/escape");
                        }

                        EditorGUIUtility.labelWidth = 180f;
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.LabelField("Actions To Run On Start Rebinding", EditorStyles.boldLabel, GUILayout.Width(230));

                    UGSActionListDrawerEditor.DrawActionList(flowEditor, false, thisAction.ActionsToRunOnStartRebinding, actionsReactivity, disableSelectionForSpecificObjectsInScene, receivedLocalVariables, isThisFieldInsideValueSelectorWindow, ref thisAction.ActionsToRunOnStartRebinding.childActionsPositionForMap);

                    GUILayout.Space(10);

                    EditorGUILayout.LabelField("Actions To Run On Cancel Rebinding", EditorStyles.boldLabel, GUILayout.Width(230));

                    UGSActionListDrawerEditor.DrawActionList(flowEditor, false, thisAction.ActionsToRunOnCancelRebinding, actionsReactivity, disableSelectionForSpecificObjectsInScene, receivedLocalVariables, isThisFieldInsideValueSelectorWindow, ref thisAction.ActionsToRunOnCancelRebinding.childActionsPositionForMap);

                    GUILayout.Space(10);

                    EditorGUILayout.LabelField("Actions To Run On Complete Rebinding", EditorStyles.boldLabel, GUILayout.Width(230));

                    UGSActionListDrawerEditor.DrawActionList(flowEditor, false, thisAction.ActionsToRunOnCompleteRebinding, actionsReactivity, disableSelectionForSpecificObjectsInScene, receivedLocalVariables, isThisFieldInsideValueSelectorWindow, ref thisAction.ActionsToRunOnCompleteRebinding.childActionsPositionForMap);

                    if (!thisAction.AllowDuplicates)
                    {
                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("Actions To Run On Duplicate Input Found", EditorStyles.boldLabel, GUILayout.Width(250));

                        UGSActionListDrawerEditor.DrawActionList(flowEditor, false, thisAction.ActionsToRunOnDuplicateInputFound, actionsReactivity, disableSelectionForSpecificObjectsInScene, receivedLocalVariables, isThisFieldInsideValueSelectorWindow, ref thisAction.ActionsToRunOnDuplicateInputFound.childActionsPositionForMap);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("  Inputs", EditorStyles.boldLabel, GUILayout.Width(200));

                //Draw the fields here
                for (int i = 0; i < thisAction.InputPreferences.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical("groupbox");
                        {
                            thisAction.InputPreferences[i].InputToRebind = EditorGUILayout.ObjectField("Input: ", thisAction.InputPreferences[i].InputToRebind, typeof(InputActionReference), true) as InputActionReference;

                            RefreshBindingOptions(thisAction.InputPreferences[i]);

                            var newSelectedBinding = EditorGUILayout.Popup(new GUIContent("Binding: "), m_SelectedBindingOption, m_BindingOptions);

                            if (newSelectedBinding != m_SelectedBindingOption)
                            {
                                var bindingId = m_BindingOptionValues[newSelectedBinding];
                                thisAction.InputPreferences[i].BindingID = bindingId;
                                m_SelectedBindingOption = newSelectedBinding;
                            }

                            if (thisAction.UseTextMeshPro)
                            {
                                thisAction.InputPreferences[i].UITextToShowCurrentInputName = EditorGUILayout.ObjectField("Current Input Name: ", thisAction.InputPreferences[i].UITextToShowCurrentInputName, typeof(TMPro.TextMeshProUGUI), true) as TMPro.TextMeshProUGUI;
                            }
                            else
                            {
                                thisAction.InputPreferences[i].UITextToShowCurrentInputName = EditorGUILayout.ObjectField("Current Input Name: ", thisAction.InputPreferences[i].UITextToShowCurrentInputName, typeof(UnityEngine.UI.Text), true) as UnityEngine.UI.Text;
                            }

                            GUILayout.Space(5);

                            thisAction.InputPreferences[i].ButtonToStartRebinding = EditorGUILayout.ObjectField("Button To Start Rebinding: ", thisAction.InputPreferences[i].ButtonToStartRebinding, typeof(UnityEngine.UI.Button), true) as UnityEngine.UI.Button;
                            thisAction.InputPreferences[i].ButtonToReset = EditorGUILayout.ObjectField("Button To Reset To Default: ", thisAction.InputPreferences[i].ButtonToReset, typeof(UnityEngine.UI.Button), true) as UnityEngine.UI.Button;

                            EditorGUILayout.BeginVertical("groupbox");
                            {
                                EditorGUILayout.LabelField("Ignored Controls", EditorStyles.boldLabel, GUILayout.Width(200));
                                GUILayout.Space(4);

                                EditorGUIUtility.labelWidth = 60f;

                                for (int j = 0; j < thisAction.InputPreferences[i].IgnoredControls.Count; j++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    thisAction.InputPreferences[i].IgnoredControls[j] = EditorGUILayout.TextField("Control: ", thisAction.InputPreferences[i].IgnoredControls[j]);

                                    if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), new GUIStyle(EditorStyles.iconButton) { margin = new RectOffset(0, 0, 2, 0) }, GUILayout.MaxWidth(25f), GUILayout.MaxHeight(25f)))
                                    {
                                        thisAction.InputPreferences[i].IgnoredControls.RemoveAt(j);
                                    }

                                    EditorGUILayout.EndHorizontal();
                                }
                                GUILayout.Space(5);
                                if (GUILayout.Button("Add Control"))
                                {
                                    thisAction.InputPreferences[i].IgnoredControls.Add("<Mouse>");
                                }

                                EditorGUIUtility.labelWidth = 180f;
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), new GUIStyle(EditorStyles.iconButton) { margin = new RectOffset(0,0,10,0)}, GUILayout.MaxWidth(25f), GUILayout.MaxHeight(25f)))
                        {
                            thisAction.InputPreferences.RemoveAt(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Add Input Preference"))
                {
                    thisAction.InputPreferences.Add(new UGSAction_SetPlayerInputPreferences.InputPreference());
                }
                EditorGUIUtility.labelWidth = 0f;

                GUILayout.Space(5);

                if(UGS_ComponentsManager.instance.language == UGS_ComponentsManager.Language.Spanish)
                {
                    EditorGUILayout.HelpBox("Recuerda usar la acción de \"Cargar Preferencias De Inputs Del Jugador\" en cada escena para cargar las preferencias.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Remember to use the \"Load Player Input Preferences\" action in each scene to load the preferences.", MessageType.Info);
                }
            }

            void RefreshBindingOptions(UGSAction_SetPlayerInputPreferences.InputPreference currentPrefs)
            {
                if (thisAction == null)
                    return;

                var actionReference = currentPrefs.InputToRebind;
                var action = actionReference?.action;

                if (action == null)
                {
                    m_BindingOptions = new GUIContent[0];
                    m_BindingOptionValues = new string[0];
                    m_SelectedBindingOption = -1;
                    return;
                }

                var bindings = action.bindings;
                var bindingCount = bindings.Count;

                m_BindingOptions = new GUIContent[bindingCount];
                m_BindingOptionValues = new string[bindingCount];
                m_SelectedBindingOption = -1;

                string currentBindingId = currentPrefs.BindingID;
                for (var i = 0; i < bindingCount; ++i)
                {
                    var binding = bindings[i];
                    var bindingId = binding.id.ToString();
                    var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

                    // If we don't have a binding groups (control schemes), show the device that if there are, for example,
                    // there are two bindings with the display string "A", the user can see that one is for the keyboard
                    // and the other for the gamepad.
                    var displayOptions =
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                    if (!haveBindingGroups)
                        displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                    // Create display string.
                    var displayString = action.GetBindingDisplayString(i, displayOptions);

                    // If binding is part of a composite, include the part name.
                    if (binding.isPartOfComposite)
                        displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

                    // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
                    // by instead using a backlash.
                    displayString = displayString.Replace('/', '\\');

                    // If the binding is part of control schemes, mention them.
                    if (haveBindingGroups)
                    {
                        var asset = action.actionMap?.asset;
                        if (asset != null)
                        {
                            var controlSchemes = string.Join(", ",
                                binding.groups.Split(InputBinding.Separator)
                                    .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                            displayString = $"{displayString} ({controlSchemes})";
                        }
                    }

                    m_BindingOptions[i] = new GUIContent(displayString);
                    m_BindingOptionValues[i] = bindingId;

                    if (currentBindingId == bindingId)
                        m_SelectedBindingOption = i;
                }
            }
        }
    }
}
