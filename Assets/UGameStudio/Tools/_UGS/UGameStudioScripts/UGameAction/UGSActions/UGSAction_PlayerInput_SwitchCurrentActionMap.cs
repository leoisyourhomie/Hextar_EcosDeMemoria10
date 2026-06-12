namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using UnityEngine;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_PlayerInput_SwitchCurrentActionMap : UGSActionNormal
        {
            [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInput));
            [field: SerializeReference] public ValueBlock RequiredValue { get; set; } = new ValueBlock_FixedTextValue ("Player");

            private PlayerInput currentPlayerInput;
            private string currentActionMapName;

            public override UGSAction Copy()
            {
                UGSAction_PlayerInput_SwitchCurrentActionMap ugsAction = (UGSAction_PlayerInput_SwitchCurrentActionMap)this.MemberwiseClone();
                //Set the ObjectSelectors or ValueBlocks that need to be copied here.
                //For example: ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredValue = RequiredValue.Copy();

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
                RequiredObject.StartReactiveBehavior();
                RequiredValue.StartReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact += RequiredObjectChanged;

                if (RequiredValue.IsReactiveOrAnchored)
                    RequiredValue.OnReact += RequiredValueChanged;

                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();
                currentActionMapName = RequiredValue.GetValue<string>();
            }

            void RequiredObjectChanged()
            {
                currentPlayerInput = RequiredObject.GetValue<PlayerInput>();
                RunAction(ComponentWhereActionIsRunning);
            }

            void RequiredValueChanged()
            {
                currentActionMapName = RequiredValue.GetValue<string>();
                RunAction(ComponentWhereActionIsRunning);
            }

            public override void StopReactiveBehavior()
            {
                RequiredObject.StopReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact -= RequiredObjectChanged;

                RequiredValue.StopReactiveBehavior();

                if (RequiredValue.IsReactiveOrAnchored)
                    RequiredValue.OnReact -= RequiredValueChanged;
            }

            protected override void Action()
            {
                if (currentPlayerInput == null)
                    return;

                if (string.IsNullOrEmpty(currentActionMapName))
                {
                    Debug.Log("[UGame Action] Switch Current Action Map: Action Map Name is null or empty. Action Ignored.");
                    return;
                }

                if (currentPlayerInput.currentActionMap.name != currentActionMapName)
                {
                    currentPlayerInput.SwitchCurrentActionMap(currentActionMapName);
                    UGameEvents.OnInputActionMapChanged?.Invoke();
                }
            }
        }
    }
}
