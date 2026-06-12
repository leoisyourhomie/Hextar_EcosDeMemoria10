namespace UGSSpace
{
    namespace UGameActions
    {
        using System;
        using System.Collections;
        using UnityEngine;
        using UnityEngine.InputSystem;

        [Serializable]
        public class UGSAction_PlayerInputManager_SetPlayerPrefab : UGSActionNormal
        {
            [field: SerializeReference] public ObjectSelector RequiredObject { get; set; } = new Select_ThisObject(typeof(PlayerInputManager));
            [field: SerializeField] public GameObject PlayerPrefab { get; set; }

            private PlayerInputManager currentPlayerInputManager;

            public override UGSAction Copy()
            {
                UGSAction_PlayerInputManager_SetPlayerPrefab ugsAction = (UGSAction_PlayerInputManager_SetPlayerPrefab)this.MemberwiseClone();
                //Set the ObjectSelectors or ValueBlocks that need to be copied here.
                //For example: ugsAction.RequiredObject = RequiredObject.Copy();
                ugsAction.RequiredObject = RequiredObject.Copy();

                return ugsAction;
            }

            public override void StartReactiveBehavior()
            {
                RequiredObject.StartReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact += RequiredObjectChanged;

                currentPlayerInputManager = RequiredObject.GetValue<PlayerInputManager>();
            }

            void RequiredObjectChanged()
            {
                currentPlayerInputManager = RequiredObject.GetValue<PlayerInputManager>();
                RunAction(ComponentWhereActionIsRunning);
            }

            public override void StopReactiveBehavior()
            {
                RequiredObject.StopReactiveBehavior();

                if (RequiredObject.IsReactiveOrAnchored)
                    RequiredObject.OnReact -= RequiredObjectChanged;
            }

            protected override void Action()
            {
                if (currentPlayerInputManager == null)
                    return;

                currentPlayerInputManager.playerPrefab = PlayerPrefab;
            }
        }
    }
}
