namespace UGSSpace
{
    
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;

    public class ValueBlock_GetNameFromInputAction : ValueBlock.GenericValueBlock<string>
    {
        public ValueBlock_GetNameFromInputAction() : base(ReactiveBehavior.Reactive, false)
        {
        }

        protected override string GetValue() => GetDisplayedInputName();
        //--------------------------------------------------------------

        [field: SerializeField] public InputActionReference InputAction { get; set; }
        [field:SerializeField] public string BindingID { get; set; }
        public override ValueBlock Copy()
        {
            ValueBlock_GetNameFromInputAction thisCopy = (ValueBlock_GetNameFromInputAction)this.MemberwiseClone();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            if(ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnInputRebinded += OnInputRebinded;
            }
        }

        private void OnInputRebinded()
        {
            OnReact?.Invoke();
        }

        public override void StopReactiveBehavior()
        {
            if (ReactAs == ReactiveBehavior.Reactive)
            {
                UGameEvents.OnInputRebinded -= OnInputRebinded;
            }
        }
        public string GetDisplayedInputName()
        {
            var displayString = "";
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            var action = InputAction?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == BindingID);
                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
                }
            }

            return displayString;
        }
    }
}
