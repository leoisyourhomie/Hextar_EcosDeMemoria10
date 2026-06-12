namespace UGSSpace
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_CustomInputIsDetectedInt : ValueBlock.GenericValueBlock<int>
    {
        public ValueBlock_Input_CustomInputIsDetectedInt() : base(ReactiveBehavior.Reactive, false)
        {
        }

        //--------------------------------------------------------------
        //The ValueBlock Value (This is the value that will be used in the function or condition)
        protected override int GetValue()
        {
            return currentValue;
        }
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeField]
        public InputAction CustomAction { get; set; } = new InputAction(" [ NAME ]");
        [field: SerializeField]
        public InputReactionTypeEnum ReactionType { get; set; } = InputReactionTypeEnum.OnPerformed;

        private int currentValue = 0;

        //--------------------------------------------------------------

        public override ValueBlock Copy()
        {
            ValueBlock_Input_CustomInputIsDetectedInt thisCopy = (ValueBlock_Input_CustomInputIsDetectedInt)this.MemberwiseClone();
            //Warning: If you have any ValueBlock or ObjectSelector, set a copy with .Copy(). For example: thisCopy.myValueBlock = this.myValueBlock.Copy();
            thisCopy.CustomAction = CustomAction.Clone();
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
                CustomAction.performed += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                CustomAction.started += InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                CustomAction.canceled += InputReacted;
            }

            CustomAction.Enable();
        }

        void InputReacted(InputAction.CallbackContext context)
        {
            if (context.valueType == typeof(int))
            {
                currentValue = context.ReadValue<int>();
            }
            else if (context.valueType == typeof(float))
            {
                currentValue = Mathf.RoundToInt(context.ReadValue<float>());
            }

            if(ReactAs ==ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        public override void StopReactiveBehavior()
        {
            if (ReactionType == InputReactionTypeEnum.OnPerformed
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                CustomAction.performed -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnStarted
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStarted
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                CustomAction.started -= InputReacted;
            }

            if (ReactionType == InputReactionTypeEnum.OnCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnStartedOrCanceled
                || ReactionType == InputReactionTypeEnum.OnPerformedOrStartedOrCanceled)
            {
                CustomAction.canceled -= InputReacted;
            }

            CustomAction.Disable();
        }
    }
}
