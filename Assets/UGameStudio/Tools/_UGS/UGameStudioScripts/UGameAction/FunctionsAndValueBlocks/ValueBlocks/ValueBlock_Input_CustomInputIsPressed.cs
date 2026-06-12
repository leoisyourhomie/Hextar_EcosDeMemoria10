namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_CustomInputIsPressed : ValueBlock.GenericValueBlock<bool>
    {
        public ValueBlock_Input_CustomInputIsPressed() : base(ReactiveBehavior.Reactive, false)
        {
        }
        //--------------------------------------------------------------
        //The ValueBlock Value (This is the value that will be used in the function or condition)
        protected override bool GetValue()
        {
            return currentValue;
        }
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeField]
        public InputAction CustomAction { get; set; } = new InputAction(" [ NAME ]");

        private bool currentValue = false;

        //--------------------------------------------------------------

        public override ValueBlock Copy()
        {
            ValueBlock_Input_CustomInputIsPressed thisCopy = (ValueBlock_Input_CustomInputIsPressed)this.MemberwiseClone();
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

            CustomAction.started += InputStarted;
            CustomAction.canceled += InputCanceled;

            CustomAction.Enable();
        }

        void InputStarted(InputAction.CallbackContext context)
        {
            currentValue = true;

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }


        void InputCanceled(InputAction.CallbackContext context)
        {
            currentValue = false;

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        public override void StopReactiveBehavior()
        {
            CustomAction.started -= InputStarted;
            CustomAction.canceled -= InputCanceled;

            CustomAction.Disable();
        }
    }
}
