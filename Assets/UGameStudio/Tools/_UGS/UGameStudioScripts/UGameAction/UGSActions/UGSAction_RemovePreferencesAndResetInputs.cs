namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using UnityEngine;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_RemovePreferencesAndResetInputs : UGSActionNormal
        {
            [field: SerializeField] public List<InputActionReference> InputActions { get; set; } = new List<InputActionReference>();

            protected override void Action()
            {
                foreach (InputActionReference input in InputActions)
                {
                    for (var i = 0; i < input.action.bindings.Count; i++)
                    {
                        input.action.RemoveBindingOverride(i);
                    }

                    PlayerPrefs.SetString(input.action.id.ToString(), input.action.SaveBindingOverridesAsJson());
                }

                UGameEvents.OnInputRebinded?.Invoke();
            }

            public override UGSAction Copy()
            {
                UGSAction_LoadPlayerInputPreferences ugsAction = (UGSAction_LoadPlayerInputPreferences)this.MemberwiseClone();

                ugsAction.InputActions = new List<InputActionReference>();
                foreach (InputActionReference inputAction in this.InputActions)
                {
                    ugsAction.InputActions.Add(inputAction);
                }

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
            }

            public override void StopReactiveBehavior()
            {
            }
        }
    }
}
