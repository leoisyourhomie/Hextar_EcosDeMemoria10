namespace UGSSpace
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    public enum SwipeDirection
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }

    [Serializable]
    public class SwipeDirectionInfo
    {
        [field: SerializeField] public SwipeDirection Direction { get; set; }
        [field: SerializeField] public float MinAngle { get; set; }
        [field: SerializeField] public float MaxAngle { get; set; }

        [field: SerializeField] public bool IsSelected { get; set; } = true;

        public SwipeDirectionInfo(SwipeDirection direction, float minAngle, float maxAngle)
        {
            Direction = direction;
            MinAngle = minAngle;
            MaxAngle = maxAngle;
        }
    }

    public class ValueBlock_Input_SwipeToDirection : ValueBlock.GenericValueBlock<Trigger>
    {
        public ValueBlock_Input_SwipeToDirection() : base(ReactiveBehavior.Reactive, false)
        {
        }

        //--------------------------------------------------------------
        //The ValueBlock Value (This is the value that will be used in the function or condition)
        protected override Trigger GetValue() => currentValue;
        //--------------------------------------------------------------
        //Fields required for the valueBlock.
        //This are the fields requiered to return the corresponding value.
        [field: SerializeField]
        public InputAction InputActionForPosition { get; set; } = new InputAction("NULL_NAME");

        [field: SerializeField]
        public InputAction InputActionForPress { get; set; } = new InputAction("NULL_NAME");

        [field: SerializeField] public float SwipeDistance { get; set; } = 0.15f; // Fraction of screen diagonal (15%)
        [field: SerializeField]
        public List<SwipeDirectionInfo> SelectableDirections { get; set; } = new()
        {
           new SwipeDirectionInfo(SwipeDirection.Up, 67.5f, 112.5f),
            new SwipeDirectionInfo(SwipeDirection.UpRight, 22.5f, 67.5f),
            new SwipeDirectionInfo(SwipeDirection.Right, 337.5f, 22.5f),
            new SwipeDirectionInfo(SwipeDirection.DownRight, 292.5f, 337.5f),
            new SwipeDirectionInfo(SwipeDirection.Down, 247.5f, 292.5f),
            new SwipeDirectionInfo(SwipeDirection.DownLeft, 202.5f, 247.5f),
            new SwipeDirectionInfo(SwipeDirection.Left, 157.5f, 202.5f),
            new SwipeDirectionInfo(SwipeDirection.UpLeft, 112.5f, 157.5f)
        };
        //--------------------------------------------------------------

        private Vector2 initialPos;
        private Vector2 currentPos => InputActionForPosition.ReadValue<Vector2>();

        bool swipeStarted = false;

        bool currentValue = false;

        public override ValueBlock Copy()
        {
            ValueBlock_Input_SwipeToDirection thisCopy = (ValueBlock_Input_SwipeToDirection)this.MemberwiseClone();
            //Warning: If you have any ValueBlock or ObjectSelector, set a copy with .Copy(). For example: thisCopy.myValueBlock = this.myValueBlock.Copy();
            thisCopy.InputActionForPosition = InputActionForPosition.Clone();
            thisCopy.InputActionForPress = InputActionForPress.Clone();
            return thisCopy;
        }

        public override void StartReactiveBehavior()
        {
            //Notes:
            //1. If the returned value of this valueBlock would be changed in game,
            //then, we need to subscribe to the event that will notify us when the value changes (but only if this block IsReactive)
            //2. If you have other ValueBlocks or ObjectSelectors in the fields, you will need to subscribe to their events too.
            //3. Remember to call OnReact?.Invoke() in the subscribed method.

            InputActionForPosition.Enable();
            InputActionForPress.Enable();
            InputActionForPress.performed += UpdatePosition;
            InputActionForPress.canceled += DetectSwipe;

            //So we can detect when it pass the distance
            InputActionForPosition.performed += DetectSwipe;
        }

        private void UpdatePosition(InputAction.CallbackContext context)
        {
            swipeStarted = true;
            initialPos = currentPos;
        }

        private void DetectSwipe(InputAction.CallbackContext context)
        {
            if (!swipeStarted)
                return;

            Vector2 delta = currentPos - initialPos;
            // Normalize the swipe vector
            float normalizedDx = delta.x / UnityEngine.Device.Screen.width;
            float normalizedDy = delta.y / UnityEngine.Device.Screen.height;

            // Calculate the normalized swipe resistance
            float normalizedSwipeResistance = Mathf.Sqrt(normalizedDx * normalizedDx + normalizedDy * normalizedDy);

            Vector2 direction = Vector2.zero;

            if (normalizedSwipeResistance >= SwipeDistance)
            {
                direction.x = Mathf.Clamp(normalizedDx, -1, 1);
                direction.y = Mathf.Clamp(normalizedDy, -1, 1);
            }

            if (direction != Vector2.zero && ReactAs == ReactiveBehavior.Reactive)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                if (angle < 0)
                {
                    angle += 360; // Make angle always positive
                }

                SwipeDirection? directionEnum = null;

                foreach (var selectableDirection in SelectableDirections)
                {
                    if (!selectableDirection.IsSelected)
                        continue;

                    float minAngle = selectableDirection.MinAngle;
                    float maxAngle = selectableDirection.MaxAngle;

                    if ((angle >= minAngle && angle < maxAngle) ||
                        (minAngle > maxAngle && (angle >= minAngle || angle < maxAngle)))
                    {
                        directionEnum = selectableDirection.Direction;
                        break;
                    }
                }

                if (directionEnum.HasValue)
                {
                    swipeStarted = false;

                    currentValue = true;
                    OnReact?.Invoke();
                    currentValue = false;
                    OnReact?.Invoke();
                }
                //This is to detect if the mouse was released and no direction was detected.
                else if (InputActionForPress.phase == InputActionPhase.Canceled)
                {
                    swipeStarted = false;
                }
            }
        }

        public override void StopReactiveBehavior()
        {
            //Unsubscribe from the events, so the subscribed method is not called anymore. (but only if this block IsReactive)
            InputActionForPosition.Disable();
            InputActionForPress.Disable();
            InputActionForPress.performed -= UpdatePosition;
            InputActionForPress.canceled -= DetectSwipe;

            InputActionForPosition.performed -= DetectSwipe;
        }
    }
}
