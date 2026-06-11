namespace Expiria3DSpace
{


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;
    using UnityEditor;
    using System.IO;

    public class CanvasSceneUtils
    {

        /// <summary>
        /// Creates a World Space Canvas with specified rotation.
        /// </summary>
        /// <param name="menuCommand">Context of the menu command.</param>
        /// <param name="rotationX">Rotation around the X-axis in degrees.</param>
        /// <param name="canvasName">Name of the Canvas GameObject.</param>
        private static void CreateWorldSpaceCanvas(MenuCommand menuCommand, float rotationX)
        {
            // Determine the parent object from the context of the menu command
            GameObject parent = null;

            if (menuCommand.context is GameObject contextGameObject)
            {
                parent = contextGameObject;
            }
            else if (Selection.activeTransform != null)
            {
                parent = Selection.activeGameObject;
            }

            // Create a new GameObject with the specified name
            GameObject canvasGO = new GameObject("World Space Canvas");

            // Add a Canvas component and set its Render Mode to World Space
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Add CanvasScaler and GraphicRaycaster for UI scaling and interaction
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Access the RectTransform component to set position, size, rotation, and scale
            RectTransform rectTransform = canvasGO.GetComponent<RectTransform>();

            // Set the local position to (0, 0.1, 0) relative to the parent
            rectTransform.localPosition = new Vector3(0f, 0.1f, 0f);

            // Set the size to 500x500
            rectTransform.sizeDelta = new Vector2(500f, 500f);

            // Set the rotation around the X-axis as specified
            rectTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

            // Set the scale to (0.002, 0.002, 0.002)
            rectTransform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

            // If a parent was determined, set it as the parent of the new Canvas
            if (parent != null)
            {
                // Check if the parent can have children (e.g., not a prefab instance or other restrictions)
                if (!EditorUtility.IsPersistent(parent.transform.root.gameObject))
                {
                    Undo.SetTransformParent(canvasGO.transform, parent.transform, "Set Parent for World Space Canvas");
                }
            }

            // Register the creation in Unity's undo system (allows undoing the action)
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create World Space Canvas");

            // Select the newly created Canvas in the Hierarchy
            Selection.activeObject = canvasGO;
        }

        /// <summary>
        /// Menu item to create a Horizontal World Space Canvas (90 rotation).
        /// </summary>
        [MenuItem("GameObject/Expiria3D/UI/World Space Canvas (Horizontal)", false, 22)]
        static void WorldSpaceCanvasHorizontal(MenuCommand menuCommand)
        {
            CreateWorldSpaceCanvas(menuCommand, 90f);
        }

        /// <summary>
        /// Menu item to create a Vertical World Space Canvas (0 rotation).
        /// </summary>
        [MenuItem("GameObject/Expiria3D/UI/World Space Canvas (Vertical)", false, 23)]
        static void WorldSpaceCanvasVertical(MenuCommand menuCommand)
        {
            CreateWorldSpaceCanvas(menuCommand, 0f);
        }
    }
}