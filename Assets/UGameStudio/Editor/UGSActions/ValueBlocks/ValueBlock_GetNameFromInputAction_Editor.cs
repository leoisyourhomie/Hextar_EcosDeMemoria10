namespace UGSSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.InputSystem;
    using System.Linq;
    using UGSSpace.UGameActions;
    public class ValueBlock_GetNameFromInputAction_Editor : FunctionsEditor.ValueBlockEditor
    {
        public override bool IsThisBlockAssigned => thisBlock != null;
        public override int Order => 0;

        //Uncomment if your block is Anchored by its reactitve properties:

        //public override ReactivePropertyInfo[] ReactiveProperties
        //{
        //    get
        //    {
        //        if (thisBlock == null)
        //            return new ReactivePropertyInfo[0];

        //        List<ReactivePropertyInfo> consoleMessageInfo = new List<ReactivePropertyInfo>();

        //        if (thisBlock.RequiredObject.IsReactiveOrAnchored)
        //            consoleMessageInfo.Add(new ReactivePropertyInfo() { propertyNameSpanish = "Obtener Nombre de Input Action", propertyNameEnglish = "Get Name From Input Action" });

        //        return consoleMessageInfo.ToArray();
        //    }
        //}

        public override string NameInBuilderList_Spanish => "Obtener Nombre de Input Action";

        public override string NameInBuilderList_English => "Get Name From Input Action";

        public override string NameInEditor_Spanish => "Obtener Nombre de Input Action";

        public override string NameInEditor_English => "Get Name From Input Action";

        public override string DescriptionSpanish => "Obtener Nombre de Input Action";

        public override string DescriptionEnglish => "Get Name From Input Action";

        public override FlowBuilderCategory Category => new ValueBlockCategory_Input();

        public override string IconName => "d_EventTrigger Icon";

        private GUIContent[] m_BindingOptions = new GUIContent[0];
        private string[] m_BindingOptionValues = new string[0];
        private int m_SelectedBindingOption = -1;

        private ValueBlock_GetNameFromInputAction thisBlock;

        public override void Draw(FlowEditor flowEditor, ActionsReactivity actionsReactivity, ValueBlock _valueBlock, bool disableSelectionForSpecificObjectsInScene, List<IUGameVariable> receivedLocalVariables, bool isThisFieldInsideValueSelectorWindow)
        {
            thisBlock ??= (ValueBlock_GetNameFromInputAction)_valueBlock;

            EditorGUIUtility.labelWidth = 40;
            thisBlock.InputAction = EditorGUILayout.ObjectField("Input: ", thisBlock.InputAction, typeof(InputActionReference), true, GUILayout.Width(180)) as InputActionReference;
            EditorGUIUtility.labelWidth = 50;
            RefreshBindingOptions();

            var newSelectedBinding = EditorGUILayout.Popup(new GUIContent("Binding: "), m_SelectedBindingOption, m_BindingOptions, GUILayout.Width(170));

            if (newSelectedBinding != m_SelectedBindingOption)
            {
                var bindingId = m_BindingOptionValues[newSelectedBinding];
                thisBlock.BindingID = bindingId;
                m_SelectedBindingOption = newSelectedBinding;
            }
            EditorGUIUtility.labelWidth = 0;
        }

        void RefreshBindingOptions()
        {
            if (thisBlock == null)
                return;

            var actionReference = thisBlock.InputAction;
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

            string currentBindingId = thisBlock.BindingID;
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
