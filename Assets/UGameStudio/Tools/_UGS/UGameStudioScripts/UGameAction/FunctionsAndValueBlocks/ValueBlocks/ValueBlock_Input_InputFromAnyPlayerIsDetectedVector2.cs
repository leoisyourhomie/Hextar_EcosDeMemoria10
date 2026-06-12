namespace UGSSpace
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_InputFromAnyPlayerIsDetectedVector2 : ValueBlock.GenericValueBlock<Vector2>
    {
        public ValueBlock_Input_InputFromAnyPlayerIsDetectedVector2() : base(ReactiveBehavior.Reactive, false)
        {
        }
        //--------------------------------------------------------------
        //The ValueBlock Value (This is the value that will be used in the function or condition)
        protected override Vector2 GetValue()
        {
            return currentValue;
        }
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeField]
        public InputActionReference InputActionRef { get; set; }
        [field: SerializeField]
        public InputReactionTypeEnum ReactionType { get; set; } = InputReactionTypeEnum.OnPerformed;

        private Vector2 currentValue = Vector2.zero;

        //--------------------------------------------------------------

        public override ValueBlock Copy()
        {
            ValueBlock_Input_InputFromAnyPlayerIsDetectedVector2 thisCopy = (ValueBlock_Input_InputFromAnyPlayerIsDetectedVector2)this.MemberwiseClone();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            //Notes:
            //1. If the returned value of this valueBlock would be changed in game,
            //then, we need to subscribe to the event that will notify us when the value changes (but only if this block IsReactive)
            //2. If you have other ValueBlocks or ObjectSelectors in the fields, you will need to subscribe to their events too.
            //3. Remember to call OnReact?.Invoke() in the subscribed method.

            if (ReactionType == InputReactionTypeEnum.OnPerformed
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.performed += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.started += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.canceled += InputReacted;
            }

            InputActionRef.action.Enable();
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

        public override void StopReactiveBehavior()
        {
            if (ReactionType == InputReactionTypeEnum.OnPerformed
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.performed -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.started -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                InputActionRef.action.canceled -= InputReacted;
            }

            InputActionRef.action.Disable();
        }
    }
}
