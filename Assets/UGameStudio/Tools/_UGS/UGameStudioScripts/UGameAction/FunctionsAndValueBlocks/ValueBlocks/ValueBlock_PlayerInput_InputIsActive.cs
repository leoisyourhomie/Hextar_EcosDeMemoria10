namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_PlayerInput_InputIsActive : ValueBlock.GenericValueBlock<bool>
    {
        public ValueBlock_PlayerInput_InputIsActive() : base(ReactiveBehavior.Reactive, false) { }

        protected override bool GetValue() => inputIsActive;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));
        //--------------------------------------------------------------

        private bool inputIsActive = false;

        public override ValueBlock Copy()
        {
            ValueBlock_PlayerInput_InputIsActive thisCopy = (ValueBlock_PlayerInput_InputIsActive)this.MemberwiseClone();
            //Warning: If you have any ValueBlock or ObjectSelector, set a copy with .Copy(). For example: thisCopy.myValueBlock = this.myValueBlock.Copy();
            thisCopy.RequiredObject = RequiredObject.Copy();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            //Notes:
            //1. If the returned value of this valueBlock would be changed in game,
            //then, we need to subscribe to the event that will notify us when the value changes (but only if this block IsReactive)
            //2. If you have other ValueBlocks or ObjectSelectors in the fields, you will need to subscribe to their events too.
            //3. Remember to call OnReact?.Invoke() in the subscribed method.

            RequiredObject.StartReactiveBehavior();

            if (RequiredObject.IsReactiveOrAnchored)
            {
                RequiredObject.OnReact += OnControlsChanged;
            }

            if (ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnInputIsActiveOptionChanged += OnControlsChanged;
            }

            UpdateInputIsActive();
        }

        void UpdateInputIsActive()
        {
            PlayerInput currentPlayerInput = RequiredObject.GetValue<PlayerInput>();

            if (currentPlayerInput != null)
            {
                inputIsActive = currentPlayerInput.inputIsActive;
            }
            else
            {
                inputIsActive = false;
            }
        }

        void OnControlsChanged()
        {
            UpdateInputIsActive();

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        public override void StopReactiveBehavior()
        {
            //Unsubscribe from the events, so the subscribed method is not called anymore. (but only if this block IsReactive)
            RequiredObject.StopReactiveBehavior();

            if (RequiredObject.IsReactiveOrAnchored)
            {
                RequiredObject.OnReact -= OnControlsChanged;
            }

            if(ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnInputIsActiveOptionChanged -= OnControlsChanged;
            }
        }
    }
}
