namespace UGSSpace
{

    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;

    public class ValueBlock_PlayerInputManager_CountLocalPlayers : ValueBlock.GenericValueBlock<int>
    {
        public ValueBlock_PlayerInputManager_CountLocalPlayers() : base(ReactiveBehavior.Reactive, false)
        {
        }

        protected override int GetValue() => currentLocalPlayers;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInputManager));
        //--------------------------------------------------------------

        private PlayerInputManager currentPlayerInputManager;
        private PlayerInputManager previousPlayerInputManager;

        private int currentLocalPlayers = 0;

        public override ValueBlock Copy()
        {
            ValueBlock_PlayerInputManager_CountLocalPlayers thisCopy = (ValueBlock_PlayerInputManager_CountLocalPlayers)this.MemberwiseClone();
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

            UpdateCurrentPlayerInputManager();
        }

        void UpdateCurrentPlayerInputManager()
        {
            previousPlayerInputManager = currentPlayerInputManager;
            currentPlayerInputManager = RequiredObject.GetValue<PlayerInputManager>();

            if (ReactAs == ReactiveBehavior.Reactive && previousPlayerInputManager != currentPlayerInputManager)
            {
                if (previousPlayerInputManager != null)
                {
                    previousPlayerInputManager.onPlayerJoined -= OnJoinOrLeft;
                    previousPlayerInputManager.onPlayerLeft -= OnJoinOrLeft;
                }

                if (currentPlayerInputManager != null)
                {
                    currentPlayerInputManager.onPlayerJoined += OnJoinOrLeft;
                    currentPlayerInputManager.onPlayerLeft += OnJoinOrLeft;
                }
            }

            if (currentPlayerInputManager != null)
            {
                currentLocalPlayers = currentPlayerInputManager.playerCount;
            }
            else
            {
                currentLocalPlayers = 0;
            }
        }

        void OnRequiredObjectChanged()
        {
            UpdateCurrentPlayerInputManager();

            if (ReactAs == ReactiveBehavior.Reactive)
                OnReact?.Invoke();
        }

        void OnJoinOrLeft(PlayerInput playerInput)
        {
            currentLocalPlayers = currentPlayerInputManager.playerCount;
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

            if (ReactAs == ReactiveBehavior.Reactive && currentPlayerInputManager != null)
            {
                currentPlayerInputManager.onPlayerJoined -= OnJoinOrLeft;
                currentPlayerInputManager.onPlayerLeft -= OnJoinOrLeft;
            }
        }
    }
}
