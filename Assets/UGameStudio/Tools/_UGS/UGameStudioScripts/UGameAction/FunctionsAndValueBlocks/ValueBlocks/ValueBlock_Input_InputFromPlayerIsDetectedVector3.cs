namespace UGSSpace
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_InputFromPlayerIsDetectedVector3 : ValueBlock.GenericValueBlock<Vector3>
    {
        public ValueBlock_Input_InputFromPlayerIsDetectedVector3() : base(ReactiveBehavior.Reactive, false)
        {
        }

        //--------------------------------------------------------------
        //The ValueBlock Value (This is the value that will be used in the function or condition)
        protected override Vector3 GetValue()
        {
            return currentValue;
        }

        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeField]
        public InputActionReference InputActionRef { get; set; }
        [field: SerializeField]

        [field: SerializeReference]
        public ObjectSelector RequiredPlayerInput { get; set; } = new Select_ThisObject(typeof(PlayerInput));

        public InputReactionTypeEnum ReactionType { get; set; } = InputReactionTypeEnum.OnPerformed;

        private InputAction currentPlayerInputAction;
        private InputAction previousPlayerInputAction;

        private Vector3 currentValue = Vector3.zero;

        //--------------------------------------------------------------

        public override ValueBlock Copy()
        {
            ValueBlock_Input_InputFromPlayerIsDetectedVector3 thisCopy = (ValueBlock_Input_InputFromPlayerIsDetectedVector3)this.MemberwiseClone();
            thisCopy.RequiredPlayerInput = RequiredPlayerInput.Copy();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            //Notes:
            //1. If the returned value of this valueBlock would be changed in game,
            //then, we need to subscribe to the event that will notify us when the value changes (but only if this block IsReactive)
            //2. If you have other ValueBlocks or ObjectSelectors in the fields, you will need to subscribe to their events too.
            //3. Remember to call OnReact?.Invoke() in the subscribed method.

            RequiredPlayerInput.StartReactiveBehavior();

            if(RequiredPlayerInput.IsReactiveOrAnchored)
                RequiredPlayerInput.OnReact += OnRequiredPlayerInputChange;

            //Enable or disable the current/previous playerInputAction
            UpdatePlayerInputAction(false);
        }

        public override void StopReactiveBehavior()
        {
            RequiredPlayerInput.StopReactiveBehavior();

            if(RequiredPlayerInput.IsReactiveOrAnchored)
                RequiredPlayerInput.OnReact -= OnRequiredPlayerInputChange;

            DisableActions(currentPlayerInputAction);
        }

        private void EnableActions(InputAction action)
        {
            if (ReactionType == InputReactionTypeEnum.OnPerformed
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.performed += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.started += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.canceled += InputReacted;
            }

            action.Enable();
        }

        private void DisableActions(InputAction action)
        {
            if (ReactionType == InputReactionTypeEnum.OnPerformed
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.performed -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.started -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                action.canceled -= InputReacted;
            }

            action.Disable();
        }

        private void UpdatePlayerInputAction(bool forceToUpdateCurrent)
        {
            //Find the IputAction in the playerInput, so we react only to the input of the player.
            previousPlayerInputAction = currentPlayerInputAction;

            if (forceToUpdateCurrent)
                currentPlayerInputAction = RequiredPlayerInput.GetValue<PlayerInput>().actions.FindAction(InputActionRef.action.id);
            else
                currentPlayerInputAction ??= RequiredPlayerInput.GetValue<PlayerInput>().actions.FindAction(InputActionRef.action.id);

            //If the action is different, then we need to disable the previous action and enable the new one.
            if (previousPlayerInputAction != currentPlayerInputAction)
            {
                if (previousPlayerInputAction != null)
                    DisableActions(previousPlayerInputAction);

                if (currentPlayerInputAction != null)
                    EnableActions(currentPlayerInputAction);
            }
        }

        private void OnRequiredPlayerInputChange()
        {
            //Update the playerInputAction
            UpdatePlayerInputAction(true);

            if(ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        void InputReacted(InputAction.CallbackContext context)
        {
            if (context.valueType == typeof(Vector3))
            {
                currentValue = context.ReadValue<Vector3>();
            }
            else if (context.valueType == typeof(Vector2))
            {
                currentValue = context.ReadValue<Vector2>();
            }

            if(ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }
    }
}
