using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Security;

namespace Expiria3DSpace
{
    public class ArAction : MonoBehaviour, IPointerClickHandler
    {
        public float delay = 0;

        [SerializeReference]
        public ArActionBase actionData;

        private bool actionStarted = false;

        public bool IsActionStarted
        {
            get { return actionStarted; }
            set { actionStarted = value; }
        }

        void Reset()
        {
            actionData = new ArAction_OpenUrl();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (actionData.StartWith == ArActionBase.StartWithEnum.OnClick)
            {
                actionStarted = true;

                //We only need to run the action if it's not set to run on update
                if (!actionData.RunOnUpdate)
                {
                    if (delay > 0)
                    {
                        Invoke("RunAction", delay);
                    }
                    else
                    {
                        actionData.Run();
                    }
                }
            }
        }

        private void OnMouseUp()
        {
            if (actionData.StartWith == ArActionBase.StartWithEnum.OnClick)
            {
                actionStarted = true;

                //We only need to run the action if it's not set to run on update
                if (!actionData.RunOnUpdate)
                {
                    if (delay > 0)
                    {
                        Invoke("RunAction", delay);
                    }
                    else
                    {
                        actionData.Run();
                    }
                }
            }
        }

        private void Awake()
        {
            actionData.Initialize(this);
        }

        private void Start()
        {
            if (actionData.StartWith == ArActionBase.StartWithEnum.OnStart)
            {
                actionStarted = true;

                //We only need to run the action if it's not set to run on update
                if (!actionData.RunOnUpdate)
                {
                    if (delay > 0)
                    {
                        Invoke("RunAction", delay);
                    }
                    else
                    {
                        actionData.Run();
                    }
                }
            }
        }

        private void RunAction()
        {
            actionData.Run();
        }

        private void Update()
        {
            if (actionData.RunOnUpdate && actionStarted)
            {
                actionData.Run();
            }
        }

        private void OnEnable()
        {
            actionData?.OnEnable();
        }

        private void OnDisable()
        {
            actionData?.OnDisable();
        }
    }

    [System.Serializable]
    public abstract class ArActionBase
    {
        public enum StartWithEnum
        {
            OnClick,
            OnStart,
        }

        public abstract void Run();
        public abstract StartWithEnum StartWith { get; }
        public abstract bool RunOnUpdate { get; }

        // Referencia al MonoBehaviour que ejecuta la acción
        protected ArAction owner;

        public void Initialize(ArAction owner)
        {
            this.owner = owner;
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }

    [System.Serializable]
    public class ArAction_PlaySound : ArActionBase
    {
        public StartWithEnum startWith = StartWithEnum.OnClick;
        public AudioClip audioClip;
        public bool loop = false;
        public bool pauseOnDisable = false; // Nueva opción

        private AudioSource audioSource;
        private bool wasPlaying = false;
        private float audioTime = 0f;

        public override StartWithEnum StartWith => startWith;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            if (audioClip == null)
            {
                Debug.LogWarning("No se ha asignado un AudioClip para reproducir.");
                return;
            }

            if (audioSource == null)
            {
                // Crear un GameObject independiente para manejar el AudioSource
                GameObject audioObject = new GameObject("AudioPlayer");
                audioSource = audioObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.loop = loop;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // Sonido 2D

                // Asegurar que el GameObject persista entre escenas si es necesario
                Object.DontDestroyOnLoad(audioObject);
            }

            audioSource.Play();
        }

        public override void OnDisable()
        {
            if (pauseOnDisable && audioSource != null)
            {
                wasPlaying = audioSource.isPlaying;
                if (wasPlaying)
                {
                    audioTime = audioSource.time;
                    audioSource.Pause();
                }
            }
        }

        public override void OnEnable()
        {
            if (pauseOnDisable && audioSource != null && wasPlaying)
            {
                audioSource.time = audioTime;
                audioSource.Play();
            }
        }
    }


    [System.Serializable]
    public class ArAction_PlayAnimation : ArActionBase
    {
        public StartWithEnum startWith = StartWithEnum.OnStart;
        public GameObject targetObject; // Objeto con Animation

        public AnimationClip originalClip; // Nuevo campo para almacenar el clip original seleccionado
        public AnimationClip animationClip; // Debe ser el clip Legacy clonado
        public bool resumeOnEnable = true;

        public enum AnimationModeEnum { Once, Loop, PingPong }
        public AnimationModeEnum animationMode = AnimationModeEnum.Loop;
        private Animation animationComponent;

        private bool wasPlaying = false;
        private float animationTime = 0f;
        private AnimationState animState;

        public override StartWithEnum StartWith => startWith;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            if (targetObject == null)
            {
                // Si no se ha asignado un objeto, usar el objeto del componente ArAction
                targetObject = owner.gameObject;
            }

            // Obtener o agregar el componente Animation
            animationComponent = targetObject.GetComponent<Animation>();
            if (animationComponent == null)
            {
                animationComponent = targetObject.AddComponent<Animation>();
            }

            if (animationClip == null)
            {
                Debug.LogWarning("No se ha seleccionado ninguna animación.");
                return;
            }

            // Asegurarse de que el clip está marcado como Legacy
            if (!animationClip.legacy)
            {
                Debug.LogError("El AnimationClip no está marcado como Legacy. Por favor, asegúrate de que el clip es Legacy.");
                return;
            }

            // Agregar el clip al componente Animation si aún no está agregado
            if (!animationComponent.GetClip(animationClip.name))
            {
                animationComponent.AddClip(animationClip, animationClip.name);
            }

            // Configurar el modo de envoltura del clip
            animationComponent[animationClip.name].wrapMode = GetWrapMode(animationMode);

            // Reproducir la animación
            animationComponent.Play(animationClip.name);
            animState = animationComponent[animationClip.name];
        }

        private WrapMode GetWrapMode(AnimationModeEnum mode)
        {
            switch (mode)
            {
                case AnimationModeEnum.Loop:
                    return WrapMode.Loop;
                case AnimationModeEnum.PingPong:
                    return WrapMode.PingPong;
                case AnimationModeEnum.Once:
                    return WrapMode.Once;
                default:
                    return WrapMode.Default;
            }
        }

        public override void OnDisable()
        {
            if (animationComponent != null && animState != null)
            {
                wasPlaying = animationComponent.isPlaying;
                animationTime = animState.time;
                animationComponent.Stop();
            }
        }

        public override void OnEnable()
        {
            if (resumeOnEnable && animationComponent != null && animState != null)
            {
                animState.time = animationTime;
                if (wasPlaying)
                {
                    animationComponent.Play(animState.name);
                }
            }
        }
    }

    [System.Serializable]
    public class ArAction_OpenUrl : ArActionBase
    {
        public string url = "http://www.example.com";

        public override void Run()
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
            else
            {
                Debug.LogWarning("No se ha especificado una URL para abrir.");
            }
        }

        public override StartWithEnum StartWith => StartWithEnum.OnClick;

        public override bool RunOnUpdate => false;
    }

    [System.Serializable]
    public class ArAction_ChangeVideoURL : ArActionBase
    {
        public PlayVideoFromURL videoPlayer;

        public string videoURL = "http://www.example.com";
        public bool playAutomatically = true;

        public override void Run()
        {
            if (videoPlayer != null)
            {
                videoPlayer.LoadVideo(videoURL, playAutomatically);
            }
            else
            {
                Debug.LogWarning("No se ha asignado un reproductor de video.");
            }
        }

        public override StartWithEnum StartWith => StartWithEnum.OnClick;

        public override bool RunOnUpdate => false;
    }


    [System.Serializable]
    public class ArAction_EnableOrDisableObjects : ArActionBase
    {
        public enum ActionEnum { Enable, Disable, Toggle }

        [System.Serializable]
        public class ObjectAction
        {
            public GameObject targetObject;
            public ActionEnum action = ActionEnum.Enable;
        }

        public List<ObjectAction> objectActions = new List<ObjectAction>(1) { new ObjectAction() };

        string ActionToName(ActionEnum action)
        {
            switch (action)
            {
                case ActionEnum.Enable:
                    return "Activar";
                case ActionEnum.Disable:
                    return "Desactivar";
                case ActionEnum.Toggle:
                    return "Alternar";
                default:
                    return "Desconocido";
            }
        }

        public override void Run()
        {
            foreach (ObjectAction objAction in objectActions)
            {
                if (objAction.targetObject != null)
                {
                    switch (objAction.action)
                    {
                        case ActionEnum.Enable:
                            objAction.targetObject.SetActive(true);
                            break;
                        case ActionEnum.Disable:
                            objAction.targetObject.SetActive(false);
                            break;
                        case ActionEnum.Toggle:
                            objAction.targetObject.SetActive(!objAction.targetObject.activeSelf);
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("No se ha asignado un objeto para " + ActionToName(objAction.action) + " en el objeto " + owner.name);
                }
            }
        }

        public override StartWithEnum StartWith => StartWithEnum.OnClick;

        public override bool RunOnUpdate => false;
    }


    [System.Serializable]
    // Acción para rotar un objeto
    public class ArAction_RotateObject : ArActionBase
    {
        public enum RotationAxis { X, Y, Z }
        public enum RotationMode { Constant, Timed, AddAngle }

        public StartWithEnum startWith = StartWithEnum.OnClick;
        public RotationAxis rotationAxis = RotationAxis.Y;
        public float rotationSpeed = 90f; // Grados por segundo
        public RotationMode rotationMode = RotationMode.Constant;
        public float rotationDuration = 1f; // Solo para Timed
        public float rotationAmount = 90f; // Solo para AddAngle
        public GameObject targetObject; // Por defecto, este objeto

        private bool isRotating = false;
        private float timeRemaining;
        private float rotationRemaining;

        public override StartWithEnum StartWith => startWith;
        public override bool RunOnUpdate => isRotating;

        public override void Run()
        {
            if (targetObject == null)
            {
                targetObject = GameObject.FindFirstObjectByType<ArAction>().gameObject;
                if (targetObject == null)
                {
                    Debug.LogWarning("No se ha encontrado el objeto para rotar.");
                    return;
                }
            }

            if (!isRotating)
            {
                if (rotationMode == RotationMode.AddAngle)
                {
                    isRotating = true;
                    rotationRemaining = rotationAmount;
                }
                else
                {
                    isRotating = true;
                    if (rotationMode == RotationMode.Timed)
                    {
                        timeRemaining = rotationDuration;
                    }
                }
            }

            if (isRotating)
            {
                float deltaRotation = rotationSpeed * Time.deltaTime;

                if (rotationMode == RotationMode.AddAngle)
                {
                    if (deltaRotation > rotationRemaining)
                    {
                        deltaRotation = rotationRemaining;
                    }

                    Rotate(deltaRotation);
                    rotationRemaining -= deltaRotation;

                    if (rotationRemaining <= 0f)
                    {
                        isRotating = false;
                    }
                }
                else
                {
                    Rotate(deltaRotation);

                    if (rotationMode == RotationMode.Timed)
                    {
                        timeRemaining -= Time.deltaTime;
                        if (timeRemaining <= 0)
                        {
                            isRotating = false;
                        }
                    }
                }
            }
        }

        private void Rotate(float angle)
        {
            Vector3 axis = Vector3.up;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    axis = Vector3.right;
                    break;
                case RotationAxis.Y:
                    axis = Vector3.up;
                    break;
                case RotationAxis.Z:
                    axis = Vector3.forward;
                    break;
            }

            targetObject.transform.Rotate(axis, angle, Space.Self);
        }
    }


    [System.Serializable]
    // Acción para mover un objeto
    public class ArAction_MoveObject : ArActionBase
    {
        public enum MovementMode { AddOffset, SetPosition, MoveToObject }

        public StartWithEnum startWith = StartWithEnum.OnClick;
        public MovementMode movementMode = MovementMode.AddOffset;
        public Vector3 positionOffset;
        public Vector3 targetPosition;
        public GameObject targetPositionObject;
        public float movementDuration = 1f; // Duración del movimiento
        public GameObject targetObject; // Por defecto, este objeto

        private Vector3 startPosition;
        private Vector3 endPosition;
        private float elapsedTime = 0f;
        private bool isMoving = false;

        public override StartWithEnum StartWith => startWith;

        // Modificamos RunOnUpdate para que dependa de isMoving o de si la acción ha iniciado
        public override bool RunOnUpdate => isMoving || ((ArAction)owner).IsActionStarted;

        public override void Run()
        {
            if (targetObject == null)
            {
                targetObject = owner.gameObject;
                if (targetObject == null)
                {
                    Debug.LogWarning("No se ha encontrado el objeto para mover.");
                    return;
                }
            }

            // Solo inicializamos el movimiento si la acción ha iniciado y no estamos moviendo
            if (!isMoving && owner.IsActionStarted)
            {
                startPosition = targetObject.transform.position;

                switch (movementMode)
                {
                    case MovementMode.AddOffset:
                        endPosition = startPosition + positionOffset;
                        break;
                    case MovementMode.SetPosition:
                        endPosition = targetPosition;
                        break;
                    case MovementMode.MoveToObject:
                        if (targetPositionObject != null)
                        {
                            endPosition = targetPositionObject.transform.position;
                        }
                        else
                        {
                            Debug.LogWarning("No se ha asignado el objeto destino para mover.");
                            return;
                        }
                        break;
                }

                isMoving = true;
                elapsedTime = 0f;
            }

            if (isMoving)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / movementDuration);
                targetObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                if (t >= 1f)
                {
                    isMoving = false;
                    // Restablecer actionStarted para permitir que la acción se pueda activar nuevamente
                    owner.IsActionStarted = false;
                }
            }
        }
    }


    [System.Serializable]
    // Acciones para realizar actividades al hacer clic (llamada, mensaje, WhatsApp, correo)
    // Acción para hacer una llamada al hacer clic
    public class ArAction_MakeCall : ArActionBase
    {
        public string phoneNumber;
        public override StartWithEnum StartWith => StartWithEnum.OnClick;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                Debug.LogWarning("No se ha especificado un número de teléfono.");
                return;
            }

            Application.OpenURL("tel://" + phoneNumber);
        }
    }

    [System.Serializable]
    // Acción para enviar un mensaje de texto al hacer clic
    public class ArAction_SendTextMessage : ArActionBase
    {
        public string phoneNumber;
        public string message;
        public override StartWithEnum StartWith => StartWithEnum.OnClick;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                Debug.LogWarning("No se ha especificado un número de teléfono.");
                return;
            }

            string url = "sms:" + phoneNumber + "?body=" + UnityWebRequest.EscapeURL(message);
            Application.OpenURL(url);
        }
    }

    [System.Serializable]
    // Acción para abrir WhatsApp al hacer clic
    public class ArAction_OpenWhatsApp : ArActionBase
    {
        public string phoneNumber;
        public string message;
        public override StartWithEnum StartWith => StartWithEnum.OnClick;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            string url = "https://api.whatsapp.com/send?phone=" + phoneNumber + "&text=" + UnityWebRequest.EscapeURL(message);
            Application.OpenURL(url);
        }
    }

    [System.Serializable]
    // Acción para enviar correo electrónico al hacer clic
    public class ArAction_SendEmail : ArActionBase
    {
        public string emailAddress;
        public string subject;
        public string body;
        public override StartWithEnum StartWith => StartWithEnum.OnClick;
        public override bool RunOnUpdate => false;

        public override void Run()
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                Debug.LogWarning("No se ha especificado una dirección de correo electrónico.");
                return;
            }

            string mailto = "mailto:" + emailAddress + "?subject=" + UnityWebRequest.EscapeURL(subject) + "&body=" + UnityWebRequest.EscapeURL(body);
            Application.OpenURL(mailto);
        }
    }
}