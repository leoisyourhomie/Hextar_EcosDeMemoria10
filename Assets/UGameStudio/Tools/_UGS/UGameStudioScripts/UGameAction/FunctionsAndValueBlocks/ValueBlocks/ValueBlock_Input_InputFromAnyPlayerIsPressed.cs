namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_InputFromAnyPlayerIsPressed : ValueBlock.GenericValueBlock<bool>
    {
        public ValueBlock_Input_InputFromAnyPlayerIsPressed() : base(ReactiveBehavior.Reactive, false)
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
        public InputActionReference InputActionRef { get; set; }

        private bool currentValue = false;

        //--------------------------------------------------------------

        public override ValueBlock Copy()
        {
            ValueBlock_Input_InputFromAnyPlayerIsPressed thisCopy = (ValueBlock_Input_InputFromAnyPlayerIsPressed)this.MemberwiseClone();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            //Notes:
            //1. If the returned value of this valueBlock would be changed in game,
            //then, we need to subscribe to the event that will notify us when the value changes (but only if this block IsReactive)
            //2. If you have other ValueBlocks or ObjectSelectors in the fields, you will need to subscribe to their events too.
            //3. Remember to call OnReact?.Invoke() in the subscribed method.

            InputActionRef.action.started += InputStarted;
            InputActionRef.action.canceled += InputCanceled;

            InputActionRef.action.Enable();
        }

        void InputStarted(InputAction.CallbackContext context)
        {
            currentValue = true;
            if(ReactAs == ReactiveBehavior.Reactive)
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
            InputActionRef.action.started -= InputStarted;
            InputActionRef.action.canceled -= InputCanceled;

            InputActionRef.action.Disable();
        }
    }
}
