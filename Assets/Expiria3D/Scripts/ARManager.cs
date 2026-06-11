using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Expiria3DSpace.World;
using TMPro;
using TriLibCore.Dae.Schema;
using UnityEngine;

namespace Expiria3DSpace
{
    public class ARManager : MonoBehaviour
    {
        public Camera mainCamera;

        // Jitter-delay tradeoff. Higher stability, higher delay
        [HideInInspector]
        public int stability = 4; // From 1 to 6. Default 4 

        [SerializeField]
        public List<ImageTracker> imageTrackers = new List<ImageTracker>();

        public string uGameProjectId = "";

        public enum ARProjectType
        {
            None,
            Image,
            World
        }

        static ARManager instance;

        public static ARManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ARManager>(true);
                }

                return instance;
            }
        }

        [HideInInspector]
        public ARProjectType arProjectType = ARProjectType.None;

        public GameObject worldPlacementIndicator;
        public GameObject worldContentParent;
        public GameObject worldModelContent;
        public GameObject worldLoadingAnimation;


        void Update()
        {
            if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                return;

            if (worldPlacementIndicator.activeSelf)
            {
                worldModelContent.transform.parent = worldPlacementIndicator.transform;
                worldLoadingAnimation.transform.parent = worldPlacementIndicator.transform;

                worldModelContent.transform.localPosition = new Vector3(0, 0, 0);
                worldLoadingAnimation.transform.localPosition = new Vector3(0, 0, 0);

                //set scale to 1
                worldModelContent.transform.localScale = new Vector3(1, 1, 1);
                worldLoadingAnimation.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                worldModelContent.transform.parent = worldContentParent.transform;
                worldLoadingAnimation.transform.parent = worldContentParent.transform;

                worldModelContent.transform.localPosition = new Vector3(0, 0, 0);
                worldLoadingAnimation.transform.localPosition = new Vector3(0, 0, 0);

                //set scale to 1
                worldModelContent.transform.localScale = new Vector3(1, 1, 1);
                worldLoadingAnimation.transform.localScale = new Vector3(1, 1, 1);
            }
        }

#if UNITY_WEBGL

        public delegate void OnTargetEvent(int targetIndex);

        public event OnTargetEvent onTargetFoundEvent;
        public event OnTargetEvent onTargetLostEvent;
        public event OnTargetEvent onTargetUpdateEvent;

        private bool autoStart = true;
        private float[,] markerDimensions;
        private bool[] isTargetVisibles;

        private bool facingUser = false;

        private Expiria3DSpace.ARCameraTarget imageARCamera;
        private Expiria3DSpace.World.ARCamera worldARCamera;

        //This is used to reload from the web browser
        public void Reload()
        {
            Debug.Log("Recargando la aplicación...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        void OnError()
        {
            if (arProjectType == ARProjectType.Image)
            {
                mainCamera.gameObject.SetActive(true);
                imageARCamera.gameObject.SetActive(false);

                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    canvas.worldCamera = mainCamera;
                }

                //Enable the first image tracker
                if (imageTrackers.Count > 0)
                {
                    imageTrackers[0].gameObject.SetActive(true);

                    //Enable the tracking image preview
                    Transform child = imageTrackers[0].transform.Find("_TargetTrackingImagePreview");
                    if (child != null)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
        }

        void Awake()
        {
            if (uGameProjectId.Length != 36)
            {
                Debug.LogError("Please set the uGameProjectId in the ARManager component. The current projectId is incorrect. Go to https://expiria3d.com/user/dashboard, select your project, and get the projectId from the URL.", this);
            }

            imageARCamera = GetComponentInChildren<Expiria3DSpace.ARCameraTarget>(true);
            worldARCamera = GetComponentInChildren<Expiria3DSpace.World.ARCamera>(true);
            WorldTracker worldTracker = GetComponentInChildren<WorldTracker>(true);

            if (arProjectType == ARProjectType.Image)
            {
                if (worldARCamera != null)
                {
                    worldARCamera.gameObject.SetActive(false);
                }

                if (worldTracker != null)
                {
                    worldTracker.gameObject.SetActive(false);
                }

                if (imageARCamera == null)
                {
                    Debug.LogError("ARCamera missing.", this);
                    return;
                }


                if (mainCamera == null)
                {
                    Debug.LogError("Main Camera missing.", this);
                    return;
                }

                // Find all Canvas in the scene
                Canvas[] canvases = FindObjectsOfType<Canvas>(true);

                if (Application.isEditor)
                {
                    mainCamera.gameObject.SetActive(true);
                    imageARCamera.gameObject.SetActive(false);

                    foreach (Canvas canvas in canvases)

                    {
                        canvas.worldCamera = mainCamera;
                    }
                }
                else
                {
                    mainCamera.gameObject.SetActive(false);
                    imageARCamera.gameObject.SetActive(true);



                    foreach (Canvas canvas in canvases)
                    {
                        canvas.worldCamera = imageARCamera.GetComponent<Camera>();
                    }
                }

                int maxTargetIndex = 0;
                foreach (ImageTracker imageTracker in imageTrackers)
                {
                    maxTargetIndex = Math.Max(maxTargetIndex, imageTracker.targetIndex);

                    if (!Application.isEditor)
                        imageTracker.gameObject.SetActive(false);
                }

                isTargetVisibles = new bool[maxTargetIndex + 1];
            }
            else if (arProjectType == ARProjectType.World)
            {
                //WARNING: Avoid to change the parent of the object in a loop because it will break the hierarchy in realtime.
                List<Transform> children = new List<Transform>();
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    Transform childObj = transform.parent.GetChild(i);
                    if (childObj != this.transform)
                    {
                        children.Add(childObj);
                    }
                }

                foreach (Transform child in children)
                {
                    child.SetParent(worldModelContent.transform);
                }

                if (imageARCamera != null)
                {
                    imageARCamera.gameObject.SetActive(false);
                }

                if (worldARCamera == null)
                {
                    Debug.LogError("ARCamera missing.", this);

                    return;
                }

                if (worldTracker == null)
                {
                    Debug.LogError("WorldTracker missing.", this);
                    return;
                }

                if (mainCamera == null)
                {
                    Debug.LogError("Main Camera missing.", this);
                    return;
                }

                Canvas[] canvases = FindObjectsOfType<Canvas>(true);

                if (Application.isEditor)
                {
                    mainCamera.gameObject.SetActive(false);
                    worldARCamera.gameObject.SetActive(true);
                    worldTracker.gameObject.SetActive(true);
                    foreach (Canvas canvas in canvases)
                    {
                        canvas.worldCamera = mainCamera;
                    }
                }
                else
                {
                    mainCamera.gameObject.SetActive(false);
                    worldARCamera.gameObject.SetActive(true);
                    worldTracker.gameObject.SetActive(true);

                    foreach (Canvas canvas in canvases)
                    {
                        canvas.worldCamera = worldARCamera.GetComponent<Camera>();
                    }
                }
            }
        }


        private void OnEnable()
        {
            if (arProjectType == ARProjectType.Image)
            {
                MindARImagePlugin.onARReadyAction += OnARReady;
                MindARImagePlugin.onARUpdateAction += OnARUpdate;
                MindARImagePlugin.onCameraConfigChangeAction += OnCameraConfigChange;
                MindARImagePlugin.onErrorAction += OnError;
            }
        }


        private void OnDisable()
        {
            if (arProjectType == ARProjectType.Image)
            {
                MindARImagePlugin.onARReadyAction -= OnARReady;
                MindARImagePlugin.onARUpdateAction -= OnARUpdate;
                MindARImagePlugin.onCameraConfigChangeAction -= OnCameraConfigChange;
                MindARImagePlugin.onErrorAction -= OnError;
            }
        }

        void OnDestroy()
        {
            if (arProjectType == ARProjectType.Image)
            {
                StopAR();
            }
        }


        void Start()
        {
            if (arProjectType == ARProjectType.Image)
            {
                if (autoStart)
                {
                    StartAR();
                }
            }
        }

        public void StopAR()
        {
            if (arProjectType == ARProjectType.Image)
            {
                if (MindARImagePlugin.IsRunning())
                {
                    MindARImagePlugin.StopAR();
                }
            }
        }

        public void StartAR()
        {
            if (arProjectType == ARProjectType.Image)
            {
                MindARImagePlugin.SetIsFacingUser(facingUser);
                MindARImagePlugin.SetMindFilePath("https://storage.ugame.ai/ar_target/" + uGameProjectId + ".mind");
                MindARImagePlugin.SetMaxTrack(imageTrackers.Count);
                MindARImagePlugin.SetFilterMinCF(0.001f);

                float filterBeta = 1000 / Mathf.Pow(10, stability); // [100, 10, 1, 0.1, 0.01, 0.001]
                MindARImagePlugin.SetFilterBeta(filterBeta);

                MindARImagePlugin.StartAR();
            }
        }


        private void OnARReady()
        {
            if (arProjectType == ARProjectType.Image)
            {
                int numTargets = MindARImagePlugin.GetNumTargets();
                markerDimensions = new float[numTargets, 2];
                for (int i = 0; i < numTargets; i++)
                {
                    markerDimensions[i, 0] = MindARImagePlugin.GetTargetWidth(i);
                    markerDimensions[i, 1] = MindARImagePlugin.GetTargetHeight(i);
                }
            }
        }

        private void OnCameraConfigChange()
        {
            if (arProjectType == ARProjectType.Image)
            {
                int videoWidth = MindARImagePlugin.GetVideoWidth();
                int videoHeight = MindARImagePlugin.GetVideoHeight();
                float[] camParams = MindARImagePlugin.GetCameraParams(); // [fov, aspect, near, far]

                imageARCamera.UpdateCameraConfig(videoWidth, videoHeight, camParams[0], camParams[2], camParams[3], false); // Image tracking doesn't flip the camera horizontally

                MindARImagePlugin.BindVideoTexture(imageARCamera.GetWebCamTexture());
            }
        }

        private void OnARUpdate(int targetIndex, int isFound)
        {
            if (arProjectType == ARProjectType.Image)
            {
                float[] worldMatrix = MindARImagePlugin.GetTargetWorldMatrix(targetIndex);
                foreach (ImageTracker imageTracker in imageTrackers)

                {
                    if (imageTracker.targetIndex == targetIndex)
                    {
                        if (isFound == 1)
                        {
                            imageTracker.gameObject.SetActive(true);
                            UpdateTargetPose(imageTracker, targetIndex, worldMatrix);
                        }
                        else
                        {
                            imageTracker.gameObject.SetActive(false);
                        }
                    }
                }

                if (isFound == 1)
                {
                    if (!isTargetVisibles[targetIndex])
                    {
                        onTargetFoundEvent?.Invoke(targetIndex);
                    }
                }
                else
                {
                    if (isTargetVisibles[targetIndex])
                    {
                        onTargetLostEvent?.Invoke(targetIndex);
                    }
                }

                onTargetUpdateEvent?.Invoke(targetIndex);
                isTargetVisibles[targetIndex] = isFound == 1;
            }
        }


        private void UpdateTargetPose(ImageTracker imageTracker, int targetIndex, float[] preprocessedMatrixArray)
        {
            if (arProjectType == ARProjectType.Image)
            {
                float markerWidth = markerDimensions[targetIndex, 0];
                float markerHeight = markerDimensions[targetIndex, 1];
                float windowDeviceRatio = 1;


                Matrix4x4 m = new Matrix4x4();
                Utils.AssignMatrix4x4FromArray(ref m, preprocessedMatrixArray);

                // Apply pre Transformation (translate by markerWidth/2 and markerHeight/2 and scale by markerWidth)
                m.m03 = m.m00 * markerWidth / 2 + m.m01 * markerHeight / 2 + m.m03;
                m.m13 = m.m10 * markerWidth / 2 + m.m11 * markerHeight / 2 + m.m13;
                m.m23 = m.m20 * markerWidth / 2 + m.m21 * markerHeight / 2 + m.m23;

                // Z-axis is reversed
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;

                Vector3 translation = Utils.GetTranslationFromMatrix(ref m);
                Quaternion rotation = Utils.GetRotationFromMatrix(ref m);
                Vector3 scale = Utils.GetScaleFromMatrix(ref m);

                // Fix incorrect rotation
                rotation = rotation * Quaternion.Euler(new Vector3(0, 180, 0));

                Vector3 newScale = new Vector3(scale.x * markerWidth / windowDeviceRatio, scale.y * markerWidth / windowDeviceRatio, scale.z * markerWidth / windowDeviceRatio);

                imageTracker.UpdatePose(translation, rotation, newScale);
            }
        }
#endif
    }

}