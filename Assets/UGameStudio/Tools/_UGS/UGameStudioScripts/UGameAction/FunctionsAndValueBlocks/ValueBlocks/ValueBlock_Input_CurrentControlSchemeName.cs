namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_Input_CurrentControlSchemeName : ValueBlock.GenericValueBlock<string>
    {
        public ValueBlock_Input_CurrentControlSchemeName() : base(ReactiveBehavior.Reactive, false)
        {
        }

        protected override string GetValue() => currentName;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));
        //--------------------------------------------------------------

        private string currentName = "";

        private PlayerInput currentPlayerInput;
        private PlayerInput previousPlayerInput;

        public override ValueBlock Copy()
        {
            ValueBlock_Input_CurrentControlSchemeName thisCopy = (ValueBlock_Input_CurrentControlSchemeName)this.MemberwiseClone();
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
                RequiredObject.OnReact += OnRequiredObjectChanged;
            }

            UpdateName();
        }

        void OnRequiredObjectChanged()
        {
            UpdateName();

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        void UpdateName()
        {
            previousPlayerInput = currentPlayerInput;
            currentPlayerInput = RequiredObject.GetValue<PlayerInput>();

            if (ReactAs == ReactiveBehavior.Reactive && currentPlayerInput != previousPlayerInput)
            {
                if (previousPlayerInput != null)
                {
                    previousPlayerInput.onControlsChanged -= OnControlsChanged;
                }

                if (currentPlayerInput != null)
                {
                    currentPlayerInput.onControlsChanged += OnControlsChanged;
                }
            }

            currentName = currentPlayerInput.currentControlScheme;
        }

        void OnControlsChanged(PlayerInput playerInput)
        {
            currentName = currentPlayerInput.currentControlScheme;
            OnReact?.Invoke();
        }

        public override void StopReactiveBehavior()
        {
            //Unsubscribe from the events, so the subscribed method is not called anymore. (but only if this block IsReactive)
            RequiredObject.StopReactiveBehavior();

            if (RequiredObject.IsReactiveOrAnchored)
            {
                RequiredObject.OnReact -= OnRequiredObjectChanged;
            }

            if (ReactAs == ReactiveBehavior.Reactive && currentPlayerInput != null)
            {
                currentPlayerInput.onControlsChanged -= OnControlsChanged;
            }
        }
    }
}
