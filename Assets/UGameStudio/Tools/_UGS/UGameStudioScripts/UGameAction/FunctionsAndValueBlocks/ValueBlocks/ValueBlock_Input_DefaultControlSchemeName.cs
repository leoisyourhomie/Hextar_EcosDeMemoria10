namespace UGSSpace
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;


    public class ValueBlock_Input_DefaultControlSchemeName : ValueBlock.GenericValueBlock<string>
    {
        public ValueBlock_Input_DefaultControlSchemeName() : base(ReactiveBehavior.NonReactiveAnchoredToItsProperties, true)
        {
        }

        protected override string GetValue() => currentName;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));

        //--------------------------------------------------------------

        private string currentName = "";

        public override ValueBlock Copy()
        {
            ValueBlock_Input_DefaultControlSchemeName thisCopy = (ValueBlock_Input_DefaultControlSchemeName)this.MemberwiseClone();
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
            PlayerInput currentPlayerInput = RequiredObject.GetValue<PlayerInput>();

            if (currentPlayerInput != null)
                currentName = currentPlayerInput.defaultControlScheme;
            else
                currentName = "";
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
