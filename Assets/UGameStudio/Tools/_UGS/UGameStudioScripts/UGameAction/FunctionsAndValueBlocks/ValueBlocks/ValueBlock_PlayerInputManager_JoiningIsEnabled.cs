namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_PlayerInputManager_JoiningIsEnabled : ValueBlock.GenericValueBlock<bool>
    {
        public ValueBlock_PlayerInputManager_JoiningIsEnabled() : base(ReactiveBehavior.Reactive, false)
        {
        }

        protected override bool GetValue() => currentValue;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInputManager));
        //--------------------------------------------------------------

        private bool currentValue = false;

        public override ValueBlock Copy()
        {
            ValueBlock_Input_CurrentActionMapName thisCopy = (ValueBlock_Input_CurrentActionMapName)this.MemberwiseClone();
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
                RequiredObject.OnReact += OnJoiningEnabledChanged;
            }

            if(ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnJoiningEnableChangedInPlayerInputManager += OnJoiningEnabledChanged;
            }

            UpdateCurrentValue();
        }

        void OnJoiningEnabledChanged()
        {
            UpdateCurrentValue();

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        void UpdateCurrentValue()
        {
            PlayerInputManager currentPlayerInputManager = RequiredObject.GetValue<PlayerInputManager>();

            if(currentPlayerInputManager != null)
            {
                currentValue = currentPlayerInputManager.joiningEnabled;
            }
            else
            {
                currentValue = false;
            }
        }

        public override void StopReactiveBehavior()
        {
            //Unsubscribe from the events, so the subscribed method is not called anymore. (but only if this block IsReactive)
            RequiredObject.StopReactiveBehavior();

            if (RequiredObject.IsReactiveOrAnchored)
            {
                RequiredObject.OnReact -= OnJoiningEnabledChanged;
            }

            if(ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnJoiningEnableChangedInPlayerInputManager -= OnJoiningEnabledChanged;
            }
        }
    }
}
