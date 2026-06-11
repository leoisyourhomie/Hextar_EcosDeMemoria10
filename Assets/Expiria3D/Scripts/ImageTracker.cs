using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expiria3DSpace
{
    public class ImageTracker : MonoBehaviour, IARTarget
    {
        [HideInInspector]
        [SerializeField]
        public int targetIndex;

        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public Orientation orientation = Orientation.Horizontal;
        public bool showTrackingImageInWeb = true;

        void Awake()
        {
            Transform child = transform.Find("_TargetTrackingImagePreview");

            if (child != null)
            {
                if (Application.isEditor)
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(showTrackingImageInWeb);
                }
            }
        }

        void Reset()
        {
            CreatePreview();
        }

        public Transform CreatePreview()
        {
            //if the child object _TargetTrackingImagePreview is not found, create it with the RectTransform component, Canvas component with Render Mode in World Space, CanvasScaler component, Graphuc Raycaster, and Image component.
            //The rect transform should have a width of 500 and a height of 500. The scale should be 0.002, 0.002, 0.002. The pivot should be 0.5, 0.5
            Transform child = transform.Find("_TargetTrackingImagePreview");
            GameObject targetTrackingImagePreview = child != null ? child.gameObject : null;
            if (targetTrackingImagePreview == null)
            {
                targetTrackingImagePreview = new GameObject("_TargetTrackingImagePreview");
                targetTrackingImagePreview.transform.SetParent(transform);
                targetTrackingImagePreview.transform.localPosition = Vector3.zero;
                targetTrackingImagePreview.transform.localRotation = Quaternion.identity;
                targetTrackingImagePreview.transform.localScale = Vector3.one;

                RectTransform rectTransform = targetTrackingImagePreview.AddComponent<RectTransform>();
                rectTransform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(500, 500);

                Canvas canvas = targetTrackingImagePreview.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = -1000;

                CanvasScaler canvasScaler = targetTrackingImagePreview.AddComponent<CanvasScaler>();
                canvasScaler.dynamicPixelsPerUnit = 100;

                targetTrackingImagePreview.AddComponent<GraphicRaycaster>();

                Image image = targetTrackingImagePreview.AddComponent<Image>();
                image.color = new Color(1, 1, 1, 1);
                image.raycastTarget = false;
                image.preserveAspect = true;

                //Set the child as hidden in the hierarchy
                targetTrackingImagePreview.hideFlags = HideFlags.HideInHierarchy;
            }

            return targetTrackingImagePreview.transform;
        }

        private void OnDrawGizmos()
        {
            if (orientation == Orientation.Horizontal)
            {
                transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            transform.localScale = new Vector3(1, 1, 1);

            // Set the Gizmo color to green for the plane
            // Gizmos.color = new Color(0, 0.2f, 1, 0.33f);
            Gizmos.color = new Color(0, 0, 0, 0.0f);

            // Define the size of the plane based on orientation
            Vector3 size = Vector3.zero;
            if (transform.rotation.eulerAngles.x > 89 && transform.rotation.eulerAngles.x < 91)
            {
                size = new Vector3(1, 0.01f, 1); // 1x1 plane in the XZ plane for horizontal orientation
            }
            else
            {
                size = new Vector3(1, 1, 0.01f); // 1x1 plane in the XY plane for vertical orientation
            }

            // A clear cube, just to be able to click on the plane
            Gizmos.DrawCube(transform.position, size);

            // Draw arrows
            // Gizmos.color = Color.blue; // Set the Gizmo color to red for arrows

            // // Calculate positions at the four corners of the plane
            // Vector3[] arrowPositions = new Vector3[4];

            // if (orientation == Orientation.Horizontal)
            // {
            //     // For horizontal, positions are in the XZ plane
            //     arrowPositions[0] = transform.position + new Vector3(0.5f, 0, 0.5f);  // Top-right corner in XZ
            //     arrowPositions[1] = transform.position + new Vector3(-0.5f, 0, 0.5f); // Top-left corner in XZ
            //     arrowPositions[2] = transform.position + new Vector3(0.5f, 0, -0.5f); // Bottom-right corner in XZ
            //     arrowPositions[3] = transform.position + new Vector3(-0.5f, 0, -0.5f);// Bottom-left corner in XZ
            // }
            // else
            // {
            //     // For vertical, positions are in the XY plane
            //     arrowPositions[0] = transform.position + new Vector3(0.5f, 0.5f, 0);  // Top-right corner in XY
            //     arrowPositions[1] = transform.position + new Vector3(-0.5f, 0.5f, 0); // Top-left corner in XY
            //     arrowPositions[2] = transform.position + new Vector3(0.5f, -0.5f, 0); // Bottom-right corner in XY
            //     arrowPositions[3] = transform.position + new Vector3(-0.5f, -0.5f, 0);// Bottom-left corner in XY
            // }

            // // Draw arrows depending on orientation
            // Vector3 arrowDirection;
            // if (orientation == Orientation.Horizontal)
            // {
            //     // Arrows point upwards for horizontal plane
            //     arrowDirection = Vector3.up;
            // }
            // else
            // {
            //     // Arrows point backward (-Z) for vertical plane
            //     arrowDirection = Vector3.back;
            // }

            // // Draw arrows from the corners of the plane
            // foreach (var arrowPosition in arrowPositions)
            // {
            //     Gizmos.DrawRay(arrowPosition, arrowDirection * 0.2f);
            // }

            // Prevent editing of the transform in the inspector
            transform.hideFlags = HideFlags.NotEditable;
        }

        public void UpdatePose(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }
    }
}
