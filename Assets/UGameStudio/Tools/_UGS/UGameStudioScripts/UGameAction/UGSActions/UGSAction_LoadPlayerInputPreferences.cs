namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using UnityEngine;
        using System.Collections.Generic;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_LoadPlayerInputPreferences : UGSActionNormal
        {
            [field: SerializeField] public List<InputActionReference> InputActions { get; set; } = new List<InputActionReference>();

            protected override void Action()
            {
                foreach (InputActionReference input in InputActions)
                {
                    input.action.LoadBindingOverridesFromJson(PlayerPrefs.GetString(input.action.id.ToString()));
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
