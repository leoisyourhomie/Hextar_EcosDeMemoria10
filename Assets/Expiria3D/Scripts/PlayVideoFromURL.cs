namespace Expiria3DSpace
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Video;
    using UnityEngine.Networking;
    using System.Runtime.InteropServices;
    using UnityEngine.UI;
    using UnityEngine.EventSystems; // Necesario para EventTrigger

    public class PlayVideoFromURL : MonoBehaviour
    {
        // Variables comunes
        public string videoURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
        public bool loop = true;
        public float playbackSpeed = 1.0f;
        public float videoVolume = 1.0f;

        // VideoPlayer variables
        private VideoPlayer videoPlayer;
        private bool isPrepared = false;
        private bool isPlaying = false; // Variable para controlar el estado de reproducción
        private bool isDragging = false; // Variable para saber si se está arrastrando el slider
        private bool isSeeking = false; // Variable para saber si se está realizando una búsqueda
        private bool isLoading = false; // Variable para saber si el video está cargando

        public bool showProgressBar = true;
        public bool showPlayButton = true;
        public bool pauseAndPlayOnVideoClick = true;
        public bool playOnStart = true;

        public Slider progressBar;
        public Button playButton;
        public Button pauseButton;
        public GameObject loadingPanel; // Panel que se muestra mientras el video carga

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0414 // The field 'PlayVideoFromURL.time' is assigned but its value is never used

        private Renderer targetRenderer;
        public int textureWidth = 640;
        public int textureHeight = 480;
        private Texture2D videoTexture;
        private bool isInitialized = false;
        private double time = 0;

        bool wasPausedByOnDisableWhilePlaying = false;

#pragma warning restore CS0414 // The field 'PlayVideoFromURL.time' is assigned but its value is never used
#pragma warning restore IDE0044 // Add readonly modifier

        // Importar funciones externas para WebGL
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void InitializeVideoTexture(
            int textureID,
            string url,
            int width,
            int height,
            float playbackRate,
            bool loop,
            bool autoPlay // Agregamos este parámetro
        );

        [DllImport("__Internal")]
        private static extern void PauseVideo();

        [DllImport("__Internal")]
        private static extern void PlayVideo();

        [DllImport("__Internal")]
        private static extern void SetVolume(float volume);

        [DllImport("__Internal")]
        private static extern void SetPlaybackRate(float rate);

        [DllImport("__Internal")]
        private static extern void SetLoopJS(bool loop);

        [DllImport("__Internal")]
        private static extern float GetVideoDuration();

        [DllImport("__Internal")]
        private static extern float GetVideoCurrentTime();

        [DllImport("__Internal")]
        private static extern void SeekVideo(float time);

        [DllImport("__Internal")]
        private static extern void ChangeVideoURL(string url, bool autoPlay);

        [DllImport("__Internal")]
        private static extern int IsVideoSeeking();

        [DllImport("__Internal")]
        private static extern int IsVideoLoaded();
#endif

        void Awake()
        {
            videoPlayer = gameObject.GetComponent<VideoPlayer>();
#if !UNITY_EDITOR && UNITY_WEBGL
            // Remover el componente VideoPlayer si existe
            if (videoPlayer != null)
            {
                Destroy(videoPlayer);
            }

            // Asignar el Renderer del GameObject
            targetRenderer = gameObject.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError("El Renderer no está asignado en el GameObject.");
            }
#else
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = loop;
            videoPlayer.playbackSpeed = playbackSpeed;

            // Configuración de salida de audio
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.SetDirectAudioVolume(0, videoVolume);

            // Suscribirse al evento frameReady para actualizar la barra de progreso
            videoPlayer.frameReady += OnFrameReady;
#endif
            // Configurar los botones de reproducción y el slider
            ConfigureUI();

            // Asegurarse de que el loadingPanel esté desactivado al inicio
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            // Iniciar la preparación del video de inmediato
            StartCoroutine(PrepareVideo(videoURL, playOnStart));
        }

        void OnEnable()
        {
            if (isPrepared && (playOnStart || wasPausedByOnDisableWhilePlaying))
            {
                Play();
            }
        }

        void Update()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            // Actualizar barra de progreso en WebGL
            if (isPrepared && isPlaying && !isDragging && !isSeeking)
            {
                UpdateProgressBar();
            }
#else
            if (isPrepared && videoPlayer.isPlaying && !isDragging && !isSeeking)
            {
                time = videoPlayer.time; // Almacenar el tiempo para reanudar desde el mismo punto
                UpdateProgressBar();
            }
#endif
        }

        void OnDisable()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized)
            {
                if (isPlaying)
                {
                    wasPausedByOnDisableWhilePlaying = true;
                }

                PauseVideo();
            }
#else
            if (videoPlayer != null)
            {
                if (isPlaying)
                {
                    wasPausedByOnDisableWhilePlaying = true;
                }
                
                videoPlayer.prepareCompleted -= OnVideoPrepared;
                // El VideoPlayer se pausa automáticamente al desactivar
            }
#endif
        }

        // Nueva función pública para cambiar la URL del video y reproducir automáticamente
        public void LoadVideo(string newURL, bool autoPlay)
        {
            videoURL = newURL;
            playOnStart = autoPlay;

            // Detener el video actual si está reproduciéndose
            if (isPlaying)
            {
                Pause();
            }

            // Reiniciar variables de estado
            isPrepared = false;
            isPlaying = false;

            // Iniciar la preparación del nuevo video
            StartCoroutine(PrepareVideo(videoURL, playOnStart));
        }

        IEnumerator PrepareVideo(string url, bool autoPlay)
        {
            isLoading = true;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }

            string finalVideoURL = url;

            // Obtener la URL prefirmada si es necesario
            if (url.Contains("storage.ugame.ai") || url.Contains("storage.expiria3d.com"))
            {
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.SetRequestHeader("X-Use-Presigned-Url", "true");
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error al obtener la URL prefirmada: " + request.error);
                    yield break;
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    PresignedUrlResponse response = JsonUtility.FromJson<PresignedUrlResponse>(jsonResponse);

                    if (response != null && !string.IsNullOrEmpty(response.fileUrl))
                    {
                        finalVideoURL = response.fileUrl;
                    }
                    else
                    {
                        Debug.LogError("Error al parsear la respuesta de la URL prefirmada");
                        yield break;
                    }
                }
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            // En WebGL
            if (!isInitialized)
            {
                videoTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                videoTexture.Apply();

                // Asigna la textura al material del objeto
                if (targetRenderer == null)
                {
                    Debug.LogError("El Renderer no está asignado.");
                    yield break;
                }
                targetRenderer.material.mainTexture = videoTexture;

                // Inicializa el video en JavaScript con autoPlay
                InitializeVideoTexture(
                    videoTexture.GetNativeTexturePtr().ToInt32(),
                    finalVideoURL,
                    textureWidth,
                    textureHeight,
                    playbackSpeed,
                    loop,
                    autoPlay // Pasamos el valor de autoPlay
                );
                isInitialized = true;
            }
            else
            {
                // Cambia la URL del video en JavaScript
                ChangeVideoURL(finalVideoURL, autoPlay);
            }

            SetVideoVolume(videoVolume);
            isPrepared = true;
            isLoading = false;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            // Si autoPlay es verdadero, iniciamos la reproducción
            if (autoPlay)
            {
                Play();
            }
#else
            // En otras plataformas
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            videoPlayer.url = finalVideoURL;
            videoPlayer.isLooping = loop;
            videoPlayer.playbackSpeed = playbackSpeed;
            SetVideoVolume(videoVolume);

            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;

            // Esperar a que el video esté preparado
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            isPrepared = true;
            isLoading = false;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            // Mostrar el primer frame si no se reproduce automáticamente
            if (!autoPlay)
            {
                videoPlayer.frame = 0;
                videoPlayer.time = 0;
                videoPlayer.Play();
                videoPlayer.Pause(); // Reproducir y pausar inmediatamente para mostrar el primer frame
            }

            if (autoPlay)
            {
                Play();
            }
#endif
        }

        void OnVideoPrepared(VideoPlayer source)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            // No es necesario en WebGL
#else
            isPrepared = true;
            isLoading = false;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            if (playOnStart)
            {
                Play();
            }
#endif
        }

        public void Play()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized && isPrepared)
            {
                PlayVideo();
                isPlaying = true;
                UpdateButtonStates();
            }
#else
            if (videoPlayer != null && isPrepared)
            {
                videoPlayer.Play();
                isPlaying = true;
                UpdateButtonStates();
            }
#endif
        }

        public void Pause()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized && isPrepared)
            {
                PauseVideo();
                isPlaying = false;
                UpdateButtonStates();
            }
#else
            if (videoPlayer != null && isPrepared)
            {
                videoPlayer.Pause();
                isPlaying = false;
                UpdateButtonStates();
            }
#endif
        }

        public void Seek(float time)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized && isPrepared)
            {
                SeekVideo(time);
                StartCoroutine(CheckSeekComplete());
            }
#else
            if (videoPlayer != null && isPrepared && videoPlayer.canSetTime)
            {
                videoPlayer.time = time;
                videoPlayer.seekCompleted += OnSeekCompleted;
            }
#endif
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        // Coroutine para esperar a que el seek se complete en WebGL
        IEnumerator CheckSeekComplete()
        {
            while (IsVideoSeeking() == 1)
            {
                yield return null;
            }
            isSeeking = false;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            Play();
        }
#else
        void OnSeekCompleted(VideoPlayer source)
        {
            videoPlayer.seekCompleted -= OnSeekCompleted;
            isSeeking = false;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            Play();
        }
#endif

        public float GetCurrentTime()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized && isPrepared)
            {
                return GetVideoCurrentTime();
            }
            return 0f;
#else
            if (videoPlayer != null && isPrepared)
            {
                return (float)videoPlayer.time;
            }
            return 0f;
#endif
        }

        public float GetDuration()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isInitialized && isPrepared)
            {
                return GetVideoDuration();
            }
            return 1f; // Evitar división por cero
#else
            if (videoPlayer != null && isPrepared && videoPlayer.frameCount > 0)
            {
                return (float)(videoPlayer.frameCount / videoPlayer.frameRate);
            }
            return 1f; // Evitar división por cero
#endif
        }

        public void SetVideoVolume(float volume)
        {
            videoVolume = volume;
#if !UNITY_EDITOR && UNITY_WEBGL
            SetVolume(volume);
#else
            if (videoPlayer != null)
            {
                videoPlayer.SetDirectAudioVolume(0, volume);
            }
#endif
        }

        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = speed;
#if !UNITY_EDITOR && UNITY_WEBGL
            SetPlaybackRate(speed);
#else
            if (videoPlayer != null)
            {
                videoPlayer.playbackSpeed = speed;
            }
#endif
        }

        public void SetLoop(bool shouldLoop)
        {
            loop = shouldLoop;
#if !UNITY_EDITOR && UNITY_WEBGL
            SetLoopJS(shouldLoop);
#else
            if (videoPlayer != null)
            {
                videoPlayer.isLooping = shouldLoop;
            }
#endif
        }

        // Funciones para UI y controles
        void ConfigureUI()
        {
            // Configurar visibilidad de elementos UI
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(showProgressBar);
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;

                // Remover el listener existente
                progressBar.onValueChanged.RemoveAllListeners();

                // Agregar EventTrigger para detectar eventos de arrastre
                EventTrigger trigger = progressBar.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = progressBar.gameObject.AddComponent<EventTrigger>();
                }
                else
                {
                    trigger.triggers.Clear(); // Asegurarse de no duplicar eventos
                }
                AddEventTriggerListener(trigger, EventTriggerType.PointerDown, OnSeekBarPointerDown);
                AddEventTriggerListener(trigger, EventTriggerType.PointerUp, OnSeekBarPointerUp);
            }

            if (playButton != null)
            {
                playButton.gameObject.SetActive(showPlayButton);
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.gameObject.SetActive(showPlayButton);
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }

            UpdateButtonStates();
        }

        void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        void OnSeekBarPointerDown(BaseEventData data)
        {
            isDragging = true;
            isSeeking = true;
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }
            Pause(); // Pausar el video mientras se arrastra
        }

        void OnSeekBarPointerUp(BaseEventData data)
        {
            isDragging = false;
            // Buscar el video a la nueva posición
            float duration = GetDuration();
            float targetTime = progressBar.value * duration;
            Seek(targetTime);
            // La reproducción se reanuda después de que la búsqueda se complete
        }

        void UpdateProgressBar()
        {
            if (progressBar != null && !isDragging && !isSeeking && !isLoading)
            {
                float duration = GetDuration();
                float currentTime = GetCurrentTime();
                if (duration > 0f)
                {
                    progressBar.value = currentTime / duration;
                }
            }
        }

        void OnPlayButtonClicked()
        {
            Play();
        }

        void OnPauseButtonClicked()
        {
            Pause();
        }

        void UpdateButtonStates()
        {
            if (playButton != null)
            {
                playButton.gameObject.SetActive(!isPlaying);
                playButton.interactable = !isLoading && !isSeeking;
            }
            if (pauseButton != null)
            {
                pauseButton.gameObject.SetActive(isPlaying);
                pauseButton.interactable = !isLoading && !isSeeking;
            }
        }

        // Manejar clic en el video para pausar/reanudar
        void OnMouseDown()
        {
            if (pauseAndPlayOnVideoClick && isPrepared && !isDragging && !isSeeking && !isLoading)
            {
                if (isPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
            }
        }

        void OnFrameReady(VideoPlayer source, long frameIdx)
        {
            // Actualizar la barra de progreso
            if (!isDragging && !isSeeking && !isLoading)
            {
                UpdateProgressBar();
            }
        }

        [Serializable]
        public class PresignedUrlResponse
        {
            public string fileUrl;
        }
    }
}