namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using UnityEngine;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_PlayerInput_DeactivateInput : UGSActionNormal
        {
            [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));

            private PlayerInput currentPlayerInput;

            public override UGSAction Copy()
            {
                UGSAction_PlayerInput_DeactivateInput ugsAction = (UGSAction_PlayerInput_DeactivateInput)this.MemberwiseClone();
                //Set the ObjectSelectors or ValueBlocks that need to be copied here.
                //For example: ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredObject = RequiredObject.Copy();

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
                RequiredObject.StartReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact += RequiredObjectChanged;

                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();
            }

            void RequiredObjectChanged()
            {
                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();
                RunAction(ComponentWhereActionIsRunning);
            }

            public override void StopReactiveBehavior()
            {
                RequiredObject.StopReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact -= RequiredObjectChanged;
            }

            protected override void Action()
            {
                if (currentPlayerInput == null)
                    return;

                if (currentPlayerInput.inputIsActive)
                {
                    currentPlayerInput.DeactivateInput();
                    UGameEvents.OnInputIsActiveOptionChanged?.Invoke();
                }
            }
        }
    }
}
