namespace UGSSpace
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ValueBlock_PlayerInput_LocalPlayerIndex : ValueBlock.GenericValueBlock<int>
    {
        public ValueBlock_PlayerInput_LocalPlayerIndex(): base(ReactiveBehavior.ReactiveAnchoredToItsProperties, false)
        {
        }

        protected override int GetValue() => currentLocalPlayerIndex;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));
        //--------------------------------------------------------------

        private int currentLocalPlayerIndex = 0;

        public override ValueBlock Copy()
        {
            ValueBlock_PlayerInput_LocalPlayerIndex thisCopy = (ValueBlock_PlayerInput_LocalPlayerIndex)this.MemberwiseClone();
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

            UpdateCurrentIndex();
        }

        void OnRequiredObjectChanged()
        {
            UpdateCurrentIndex();

            if(ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        void UpdateCurrentIndex()
        {
            PlayerInput currPlayerIput = RequiredObject.GetValue<PlayerInput>();

            if (currPlayerIput != null)
            {
                currentLocalPlayerIndex = currPlayerIput.playerIndex;
            }
            else
            {
                currentLocalPlayerIndex = -1;
            }
        }

        public override void StopReactiveBehavior()
        {
            //Unsubscribe from the events, so the subscribed method is not called anymore. (but only if this block IsReactive)
            RequiredObject.StopReactiveBehavior();

            if (RequiredObject.IsReactiveOrAnchored)
            {
                RequiredObject.OnReact -= OnRequiredObjectChanged;
            }
        }
    }
}
