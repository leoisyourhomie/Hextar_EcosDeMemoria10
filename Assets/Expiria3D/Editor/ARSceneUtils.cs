using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Expiria3DSpace.UGSService.Controller;


namespace Expiria3DSpace
{
    internal static class ARSceneUtils
    {
        public static ARManager CreateARManager()
        {
            //if an ARManager already exists, don't create another one
            if (GameObject.FindFirstObjectByType<ARManager>() != null)
            {
                Debug.LogWarning("An AR Manager already exists in the scene. Please remove it before adding another one.");
                return null;
            }

            var session = ObjectFactory.CreateGameObject("AR Manager", typeof(ARManager));

            ARManager arImageTrackingManager = session.GetComponent<ARManager>();

            var arCamera = CreateARMainCamera();
            Place(arCamera.gameObject, session.transform);

            var webcamBackground = CreateWebcamBackground();
            Place(webcamBackground.gameObject, arCamera.transform);
            webcamBackground.transform.localPosition = new Vector3(0, 0, 1000);

            arCamera.gameObject.GetComponent<ARCameraTarget>().webcamQuad = webcamBackground;

            //Find the main camera, different from the AR camera
            Camera[] cameras = GameObject.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Camera camera in cameras)
            {
                if (camera.name == "AR Camera")
                {
                    continue;
                }

                arImageTrackingManager.mainCamera = camera;

                //if the name of the camera is Main Camera, rename it as Preview Camera
                if (camera.name == "Main Camera")
                {
                    camera.name = "Preview Camera";
                    camera.transform.rotation = Quaternion.Euler(25, 0, 0);
                    camera.transform.position = new Vector3(0, 1.2f, -2);
                }
                break;
            }

            if (arCamera != null)
            {
                arCamera.gameObject.SetActive(false);
                arCamera.gameObject.hideFlags = HideFlags.HideInHierarchy;

                //set dirty
                EditorUtility.SetDirty(arCamera);
            }

            session.transform.hideFlags = HideFlags.NotEditable;
            arImageTrackingManager.uGameProjectId = User.CurrentSessionInfo.project_id;

            if (ArWindow.AdvancedMode)
            {
                arImageTrackingManager.gameObject.hideFlags = HideFlags.None;
            }
            else
                arImageTrackingManager.gameObject.hideFlags = HideFlags.HideInHierarchy;

            //if eventSystem with StandardInputModule is not present, create it
            if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

                if (ArWindow.AdvancedMode)
                {
                    eventSystem.hideFlags = HideFlags.None;
                }
                else
                    eventSystem.hideFlags = HideFlags.HideInHierarchy;

                EditorUtility.SetDirty(eventSystem);
            }

            //mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return arImageTrackingManager;
        }

        static Camera CreateARMainCamera()
        {
            var cameraGo = ObjectFactory.CreateGameObject("AR Camera", typeof(Camera), typeof(ARCameraTarget));

            cameraGo.tag = "MainCamera";

            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;

            //set dirty
            EditorUtility.SetDirty(camera);

            return camera;
        }

        static GameObject CreateWebcamBackground()
        {
            var gameObject = ObjectFactory.CreateGameObject("Webcam Background");

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            meshRenderer.sharedMaterial.renderQueue = 1998;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3( 0.5f, -0.5f, 0),
                new Vector3(-0.5f,  0.5f, 0),
                new Vector3( 0.5f,  0.5f, 0)
            };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            mesh.uv = uv;

            meshFilter.mesh = mesh;

            return gameObject;
        }

        static void Place(GameObject go, Transform parent)
        {
            var transform = go.transform;

            if (parent != null)
            {
                go.transform.parent = parent;
                ResetTransform(transform);
                go.layer = parent.gameObject.layer;
            }
            else
            {
                // Puts it at the scene pivot, and otherwise world origin if there is no Scene view
                var view = SceneView.lastActiveSceneView;
                if (view != null)
                    view.MoveToView(transform);
                else
                    transform.position = Vector3.zero;

                StageUtility.PlaceGameObjectInCurrentStage(go);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(go);
        }

        static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (transform.parent is RectTransform)
            {
                var rectTransform = transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = Vector2.zero;
                }
            }
        }
    }
}