namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using UnityEngine;
        using UnityEngine.InputSystem;
        using UnityEngine.UI;

        [Serializable]
        public class UGSAction_SetPlayerInputPreferences : UGSActionNormal
        {
            [Serializable]
            public class InputPreference
            {
                [field: SerializeField] public InputActionReference InputToRebind { get; set; }

                [field: SerializeField] public string BindingID { get; set; }

                [field: SerializeField] public UnityEngine.Object UITextToShowCurrentInputName { get; set; }

                [field: SerializeField] public Button ButtonToStartRebinding { get; set; }

                [field: SerializeField] public Button ButtonToReset { get; set; }

                [field: SerializeField] public List<string> IgnoredControls { get; set; } = new List<string>() { "<Mouse>" };

                //The operation that is currently rebinding the input.
                //This stores the operation to be able to cancel it if needed.
                [NonSerialized]
                public InputActionRebindingExtensions.RebindingOperation rebindingOperation;

                public bool IsRebinding => rebindingOperation != null && rebindingOperation.started;


                private UGSAction_SetPlayerInputPreferences _owner;

                public InputPreference Copy()
                {
                    InputPreference copy = new InputPreference();
                    copy.InputToRebind = InputToRebind;
                    copy.BindingID = BindingID;
                    copy.ButtonToStartRebinding = ButtonToStartRebinding;
                    copy.ButtonToReset = ButtonToReset;
                    copy.IgnoredControls = new List<string>(IgnoredControls);

                    return copy;
                }

                public void StartReactiveBehaviour(UGSAction_SetPlayerInputPreferences owner)
                {
                    _owner = owner;

                    ButtonToStartRebinding.onClick.AddListener(StartRebinding);
                    ButtonToReset.onClick.AddListener(ResetToDefault);

                    InputSystem.onActionChange += OnActionChange;
                    UGameEvents.OnInputRebinded += UpdateInputNameInText;

                    UpdateInputNameInText();
                }

                private bool CheckDuplicateBinding(InputAction action, int bindingIndex, bool allCompositeParts = false)
                {
                    InputBinding newBinding = action.bindings[bindingIndex];

                    foreach (InputBinding binding in action.actionMap.bindings)
                    {
                        if (binding.action == newBinding.action)
                        {
                            continue;
                        }

                        if (binding.effectivePath == newBinding.effectivePath)
                        {
                            Debug.Log("Duplicate binding found: " + newBinding.overridePath);

                            foreach(UGSAction a in _owner.ActionsToRunOnDuplicateInputFound.UGSActionList)
                            {
                                a.RunAction(_owner.ComponentWhereActionIsRunning);
                            }

                            return true;
                        }
                    }

                    if (allCompositeParts)
                    {
                        for (int i = 1; i < bindingIndex; i++)
                        {
                            if (action.bindings[i].effectivePath == newBinding.overridePath)
                            {
                                Debug.Log("Duplicate binding found: " + newBinding.overridePath);

                                foreach (UGSAction a in _owner.ActionsToRunOnDuplicateInputFound.UGSActionList)
                                {
                                    a.RunAction(_owner.ComponentWhereActionIsRunning);
                                }

                                return true;
                            }
                        }
                    }

                    return false;
                }

                public void StopReactiveBehaviour()
                {
                    ButtonToStartRebinding.onClick.RemoveListener(StartRebinding);
                    ButtonToReset.onClick.RemoveListener(ResetToDefault);

                    InputSystem.onActionChange -= OnActionChange;
                    UGameEvents.OnInputRebinded -= UpdateInputNameInText;

                    rebindingOperation?.Cancel();
                    StopRebinding();
                }

                // When the action system re-resolves bindings, we want to update our UI in response. While this will
                // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
                // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
                // will update our UI to reflect the current keyboard layout.
                void OnActionChange(object obj, InputActionChange change)
                {
                    if (change != InputActionChange.BoundControlsChanged)
                        return;

                    var action = obj as InputAction;
                    var actionMap = action?.actionMap ?? obj as InputActionMap;
                    var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

                    var referencedAction = InputToRebind?.action;
                    if (referencedAction == null)
                        return;

                    if (referencedAction == action ||
                        referencedAction.actionMap == actionMap ||
                        referencedAction.actionMap?.asset == actionAsset)
                        UpdateInputNameInText();
                }

                private void StartRebinding()
                {
                    if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                        return;

                    // If the binding is a composite, we need to rebind each part in turn.
                    if (action.bindings[bindingIndex].isComposite)
                    {
                        var firstPartIndex = bindingIndex + 1;
                        if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
                    }
                    else
                    {
                        PerformInteractiveRebind(action, bindingIndex);
                    }

                    foreach (UGSAction a in _owner.ActionsToRunOnStartRebinding.UGSActionList)
                    {
                        a.RunAction(_owner.ComponentWhereActionIsRunning);
                    }
                }

                private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
                {
                    if (rebindingOperation != null && rebindingOperation.started)
                        return;

                    if (_owner.Panel != null)
                        _owner.Panel.SetActive(true);

                    if (InputToRebind == null)
                        return;

                    action.Disable();

                    rebindingOperation = action.PerformInteractiveRebinding()
                        .OnMatchWaitForAnother(0.1f)
                        .OnCancel(operation => OnCancelRebinding())
                        .OnApplyBinding((operation, newInputPath) =>
                        {
                            action.ApplyBindingOverride(bindingIndex, newInputPath);
                        })
                        .OnComplete(operation =>
                        {
                            PlayerPrefs.SetString(action.id.ToString(), action.SaveBindingOverridesAsJson());

                            if (!_owner.AllowDuplicates)
                            {
                                if (CheckDuplicateBinding(action, bindingIndex, allCompositeParts))
                                {
                                    action.RemoveBindingOverride(bindingIndex);
                                    StopRebinding();
                                    PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                                    return;
                                }
                            }

                            OnCompleteRebinding();

                            // If there's more composite parts we should bind, initiate a rebind
                            // for the next part.
                            if (allCompositeParts)
                            {
                                var nextBindingIndex = bindingIndex + 1;
                                if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                    PerformInteractiveRebind(action, nextBindingIndex, true);
                                else
                                {
                                    //Run only when finish ALL the inputs
                                    foreach (UGSAction a in _owner.ActionsToRunOnCompleteRebinding.UGSActionList)
                                    {
                                        a.RunAction(_owner.ComponentWhereActionIsRunning);
                                    }
                                }
                            }
                            else
                            {
                                //This is executed when only one input is binded
                                foreach (UGSAction a in _owner.ActionsToRunOnCompleteRebinding.UGSActionList)
                                {
                                    a.RunAction(_owner.ComponentWhereActionIsRunning);
                                }
                            }
                        });

                    for (int i = 0; i < _owner.ControlsToCancelOperation.Count; i++)
                    {
                        rebindingOperation.WithCancelingThrough(_owner.ControlsToCancelOperation[i]);
                    }

                    for (int i = 0; i < IgnoredControls.Count; i++)
                    {
                        rebindingOperation.WithControlsExcluding(IgnoredControls[i]);
                    }

                    // If it's a part binding, show the name of the part in the UI.
                    var partName = default(string);
                    if (action.bindings[bindingIndex].isPartOfComposite)
                        partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

                    // Bring up rebind overlay, if we have one.
                    if (_owner.WaitingForInput_TextInPanel != null)
                    {
                        var text = !string.IsNullOrEmpty(rebindingOperation.expectedControlType)
                            ? $"{partName}Waiting for {rebindingOperation.expectedControlType} input..."
                            : $"{partName}Waiting for input...";

                        if (_owner.WaitingForInput_TextInPanel is TMPro.TextMeshProUGUI txtPro)
                        {
                            txtPro.text = text;
                        }
                        else if (_owner.WaitingForInput_TextInPanel is UnityEngine.UI.Text txt)
                        {
                            txt.text = text;
                        }
                    }

                    rebindingOperation.Start();
                }

                private void OnCompleteRebinding()
                {
                    Debug.Log(GetDisplayedInputName() + " Rebinded");
                    StopRebinding();
                }

                private void OnCancelRebinding()
                {
                    Debug.Log(GetDisplayedInputName() + " Rebinding Cancelled");
                    StopRebinding();

                    foreach (UGSAction a in _owner.ActionsToRunOnCancelRebinding.UGSActionList)
                    {
                        a.RunAction(_owner.ComponentWhereActionIsRunning);
                    }
                }

                void StopRebinding()
                {
                    if (_owner.Panel != null)
                        _owner.Panel.SetActive(false);

                    InputToRebind.action.Enable();

                    rebindingOperation?.Dispose();
                    rebindingOperation = null;

                    //This update the text of the input
                    UGameEvents.OnInputRebinded?.Invoke();
                }

                public void UpdateInputNameInText()
                {
                    if (UITextToShowCurrentInputName is TMPro.TextMeshProUGUI)
                    {
                        TMPro.TextMeshProUGUI text = (TMPro.TextMeshProUGUI)UITextToShowCurrentInputName;
                        text.text = GetDisplayedInputName();
                    }
                    else if (UITextToShowCurrentInputName is UnityEngine.UI.Text)
                    {
                        UnityEngine.UI.Text text = (UnityEngine.UI.Text)UITextToShowCurrentInputName;
                        text.text = GetDisplayedInputName();
                    }
                }

                public void ResetToDefault()
                {
                    if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                        return;

                    if (action.bindings[bindingIndex].isComposite)
                    {
                        // It's a composite. Remove overrides from part bindings.
                        for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                            action.RemoveBindingOverride(i);
                    }
                    else
                    {
                        action.RemoveBindingOverride(bindingIndex);
                    }

                    PlayerPrefs.SetString(action.id.ToString(), action.SaveBindingOverridesAsJson());

                    UGameEvents.OnInputRebinded?.Invoke();

                    Debug.Log(GetDisplayedInputName() + " Reset to default");
                }

                public bool ResolveActionAndBinding(out InputAction action, out int _bindingIndex)
                {
                    _bindingIndex = -1;

                    action = InputToRebind?.action;
                    if (action == null)
                        return false;

                    if (string.IsNullOrEmpty(BindingID))
                        return false;

                    // Look up binding index.
                    var bindingId = new Guid(BindingID);
                    _bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
                    if (_bindingIndex == -1)
                    {
                        Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'");
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// Trigger a refresh of the currently displayed binding.
                /// </summary>
                public string GetDisplayedInputName()
                {
                    var displayString = string.Empty;
                    var deviceLayoutName = default(string);
                    var controlPath = default(string);

                    // Get display string from action.
                    var action = InputToRebind?.action;
                    if (action != null)
                    {
                        var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == BindingID);
                        if (bindingIndex != -1)
                        {
                            displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
                        }

                    }

                    return displayString;
                }
            }


            [Serializable]
            public class ActionListContainer : IUGSActionListContainer
            {
                [field: SerializeReference]
                public List<UGSAction> UGSActionList { get; set; } = new List<UGSAction>();

                [SerializeField] public Vector2 childActionsPositionForMap = new Vector2(0, 0);

                public float CurrentAnimationTimeForEditor { get; set; }

                public bool UGSActionListIsForChildClass => true;

                public void AnimateReactionInEditor()
                {
                    if (Application.isEditor)
                        CurrentAnimationTimeForEditor = Reactive.REACTIVITY_ANIMATION_DURATION;
                }
            }

            [field: SerializeField] public List<InputPreference> InputPreferences { get; set; } = new List<InputPreference>();

            [field: SerializeField] public bool UseTextMeshPro { get; set; } = true;

            /// <summary>
            /// Panel to be enabled when the rebinding operation starts.
            /// </summary>
            [field: SerializeField] public GameObject Panel { get; set; }

            /// <summary>
            /// Store the UI Text or TMPro Text to show the message "Waiting for input..."
            /// </summary>
            [field: SerializeField] public UnityEngine.Object WaitingForInput_TextInPanel { get; set; }

            [field: SerializeField] public List<string> ControlsToCancelOperation { get; set; } = new List<string>() { "<Keyboard>/escape" };

            [field: SerializeReference] public ActionListContainer ActionsToRunOnStartRebinding { get; set; } = new ActionListContainer();

            [field: SerializeReference] public ActionListContainer ActionsToRunOnCompleteRebinding { get; set; } = new ActionListContainer();

            [field: SerializeReference] public ActionListContainer ActionsToRunOnCancelRebinding { get; set; } = new ActionListContainer();

            //Actions to run when the input is already being used by another action
            [field: SerializeReference] public ActionListContainer ActionsToRunOnDuplicateInputFound { get; set; } = new ActionListContainer();

            [field: SerializeField] public bool AllowDuplicates { get; set; } = false;
            public override UGSAction Copy()
            {
                UGSAction_SetPlayerInputPreferences ugsAction = (UGSAction_SetPlayerInputPreferences)this.MemberwiseClone();
                //Set the ObjectSelectors or ValueBlocks that need to be copied here.
                //For example: ugsAction.RequiredObject = RequiredObject.Copy();

                for (int i = 0; i < InputPreferences.Count; i++)
                {
                    ugsAction.InputPreferences[i] = InputPreferences[i].Copy();
                }

                for (int i = 0; i < ActionsToRunOnStartRebinding.UGSActionList.Count; i++)
                {
                    ugsAction.ActionsToRunOnStartRebinding.UGSActionList[i] = ActionsToRunOnStartRebinding.UGSActionList[i].Copy();
                }

                for (int i = 0; i < ActionsToRunOnCompleteRebinding.UGSActionList.Count; i++)
                {
                    ugsAction.ActionsToRunOnCompleteRebinding.UGSActionList[i] = ActionsToRunOnCompleteRebinding.UGSActionList[i].Copy();
                }

                for (int i = 0; i < ActionsToRunOnCancelRebinding.UGSActionList.Count; i++)
                {
                    ugsAction.ActionsToRunOnCancelRebinding.UGSActionList[i] = ActionsToRunOnCancelRebinding.UGSActionList[i].Copy();
                }

                if (!AllowDuplicates)
                {
                    for (int i = 0; i < ActionsToRunOnDuplicateInputFound.UGSActionList.Count; i++)
                    {
                        ugsAction.ActionsToRunOnDuplicateInputFound.UGSActionList[i] = ActionsToRunOnDuplicateInputFound.UGSActionList[i].Copy();
                    }
                }

                ugsAction.ControlsToCancelOperation = new List<string>(ControlsToCancelOperation);

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
                for (int i = 0; i < InputPreferences.Count; i++)
                {
                    InputPreferences[i].StartReactiveBehaviour(this);
                }
            }

            public override void StopReactiveBehavior()
            {
                for (int i = 0; i < InputPreferences.Count; i++)
                {
                    InputPreferences[i].StopReactiveBehaviour();
                }
            }

            protected override void StopChildActions()
            {
                for (int i = 0; i < ActionsToRunOnCompleteRebinding.UGSActionList.Count; i++)
                {
                    ActionsToRunOnCompleteRebinding.UGSActionList[i].StopCoroutineAndReactiveBehaviour();
                }

                for (int i = 0; i < ActionsToRunOnCancelRebinding.UGSActionList.Count; i++)
                {
                    ActionsToRunOnCancelRebinding.UGSActionList[i].StopCoroutineAndReactiveBehaviour();
                }

                for (int i = 0; i < ActionsToRunOnStartRebinding.UGSActionList.Count; i++)
                {
                    ActionsToRunOnStartRebinding.UGSActionList[i].StopCoroutineAndReactiveBehaviour();
                }

                for (int i = 0; i < ActionsToRunOnDuplicateInputFound.UGSActionList.Count; i++)
                {
                    ActionsToRunOnDuplicateInputFound.UGSActionList[i].StopCoroutineAndReactiveBehaviour();
                }
            }

            protected override void Action()
            {
            }
        }
    }
}
