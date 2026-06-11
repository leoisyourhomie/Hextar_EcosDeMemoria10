using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Expiria3DSpace.World
{
    public class SwipeToRotateY : MonoBehaviour
    {
        [SerializeField] private Transform rotTransform;
        //TODO: implement using Screen.DPI / Canvas.innerWidth.Height API grabber

        private Vector2 startDragPos;
        private bool isDragging = false;

        private Quaternion origRot, startRot;

        [SerializeField] private float sensitivity = 20;

        void Awake()
        {
            origRot = rotTransform.rotation;
        }

#if ENABLE_INPUT_SYSTEM
        void Update()
        {
            // When multiple touches exist, cancel drag.
            if (Touchscreen.current != null)
            {
                int activeTouchCount = 0;
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var phase = Touchscreen.current.touches[i].phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                        phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    {
                        activeTouchCount++;
                    }
                }
                if (activeTouchCount > 1)
                {
                    isDragging = false;
                    return;
                }
            }

            // If a new press is detected (mouse or touch), check if it is over a UI element.
            Vector2 pointerPos = Vector2.zero;
            bool pointerPressed = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerPos = Mouse.current.position.ReadValue();
                pointerPressed = true;
            }
            else if (Touchscreen.current != null &&
                     Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
                pointerPressed = true;
            }

            if (pointerPressed && IsPointerOverUI(pointerPos))
            {
                // If we started over a UI element, cancel processing.
                isDragging = false;
                return;
            }

            // Process the beginning of a drag.
            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null &&
                 Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
            {
                if (Application.isMobilePlatform && Touchscreen.current != null)
                {
                    startDragPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
                else if (Mouse.current != null)
                {
                    startDragPos = Mouse.current.position.ReadValue();
                }

                startRot = rotTransform.rotation;
                isDragging = true;
            }
            // End drag.
            else if ((Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
                     (Touchscreen.current != null &&
                      Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended))
            {
                isDragging = false;
            }

            // Continue rotating while dragging.
            if (isDragging)
            {
                Vector2 curDragPos = Vector2.zero;
                if (Application.isMobilePlatform && Touchscreen.current != null)
                {
                    curDragPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
                else if (Mouse.current != null)
                {
                    curDragPos = Mouse.current.position.ReadValue();
                }

                float deltaX = curDragPos.x - startDragPos.x;
                float angle = deltaX * sensitivity * -1;
                rotTransform.rotation = startRot * Quaternion.AngleAxis(angle, Vector3.up);
            }
        }
#else
        void Update()
        {
            // For the legacy Input system, check if the click started over a UI element.
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUI(Input.mousePosition))
                {
                    isDragging = false;
                    return;
                }
                startDragPos = Input.mousePosition;
                startRot = rotTransform.rotation;
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector2 curDragPos = Input.mousePosition;
                float deltaX = curDragPos.x - startDragPos.x;
                float angle = deltaX * sensitivity * -1;
                rotTransform.rotation = startRot * Quaternion.AngleAxis(angle, Vector3.up);
            }
        }
#endif

        /// <summary>
        /// Returns true if the given screen position overlaps a UI element (with Raycast Target enabled).
        /// </summary>
        private bool IsPointerOverUI(Vector2 pointerPosition)
        {
            if (EventSystem.current == null)
                return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = pointerPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        public void Reset()
        {
            rotTransform.rotation = origRot;
        }
    }
}
