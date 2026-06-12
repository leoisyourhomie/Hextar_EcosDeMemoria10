namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using UnityEngine;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_PlayerInput_SwitchCurrentControlScheme : UGSActionNormal
        {
            [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));
            [field: SerializeReference] public ValueBlock RequiredValue { get; set; } = new ValueBlock_FixedTextValue("Keyboard&Mouse");

            private PlayerInput currentPlayerInput;
            private string currentControlScheme;

            public override UGSAction Copy()
            {
                UGSAction_PlayerInput_SwitchCurrentControlScheme ugsAction = (UGSAction_PlayerInput_SwitchCurrentControlScheme)this.MemberwiseClone();
                //Set the ObjectSelectors or ValueBlocks that need to be copied here.
                //For example: ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredValue = RequiredValue.Copy();

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
                RequiredObject.StartReactiveBehavior();
                RequiredValue.StartReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact += RequiredObjectChanged;

                if (RequiredValue.IsReactiveOrAnchored)
                    RequiredValue.OnReact += RequiredValueChanged;

                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();

                currentControlScheme = RequiredValue.GetValue<string>();
            }

            void RequiredObjectChanged()
            {
                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();
                RunAction(ComponentWhereActionIsRunning);
            }

            void RequiredValueChanged()
            {
                currentControlScheme = RequiredValue.GetValue<string>();
                RunAction(ComponentWhereActionIsRunning);
            }

            public override void StopReactiveBehavior()
            {
                RequiredObject.StopReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact -= RequiredObjectChanged;

                RequiredValue.StopReactiveBehavior();

                if (RequiredValue.IsReactiveOrAnchored)
                    RequiredValue.OnReact -= RequiredValueChanged;
            }

            protected override void Action()
            {
                if (currentPlayerInput == null)
                    return;

                if (string.IsNullOrEmpty(currentControlScheme))
                {
                    Debug.Log("[UGame Action] Switch Current Action Map: Action Map Name is null or empty. Action Ignored.");
                    return;
                }

                if (currentPlayerInput.currentControlScheme != currentControlScheme)
                {
                    currentPlayerInput.SwitchCurrentControlScheme(currentControlScheme);
                }
            }
        }
    }
}
