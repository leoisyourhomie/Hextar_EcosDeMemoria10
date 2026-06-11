namespace Expiria3DSpace
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Networking;
    using System.Collections;
    using System.Collections.Generic;
    using System;
    using Expiria3DSpace.UGSService.Controller;

    [Serializable]
    public class WebProject
    {
        public string[] ar_images;
        public string url_id;
        public string type;
    }

    [Serializable]
    public class WebProjectList
    {
        public WebProject[] items;
    }

    public class ArTargetsLoader
    {
        private const string SUPABASE_URL = "https://tpwwnbjhlnyoyeyrplkb.supabase.co/rest/v1/web_projects?select=ar_images,url_id,type";
        private const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRwd3duYmpobG55b3lleXJwbGtiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDE3NDk3MDgsImV4cCI6MjAxNzMyNTcwOH0.Mfr93JX7mtjsGgThB2rj_yHZByxWu0x79K4MpsVK9YQ"; // Reemplaza con tu clave real


        // Lista para almacenar los Sprites cargados
        public static List<Sprite> loadedSprites = new List<Sprite>();

        public enum ArTargetsLoaderState
        {
            None,
            Loading,
            Success,
            SuccessWithNoImages,
            Error,
        }

        public static ArTargetsLoaderState LoadingState { get; private set; } = ArTargetsLoaderState.None;

        // Variables para rastrear el progreso y errores
        private static int imagesProcessed = 0;
        private static int totalImages = 0;
        private static bool hasErrorOccurred = false;

        public static void LoadImagesAndType()
        {
            // Si ya se está cargando, retornar
            if (LoadingState == ArTargetsLoaderState.Loading)
            {
                return;
            }

            loadedSprites = new List<Sprite>();
            LoadingState = ArTargetsLoaderState.Loading;

            // Reiniciar variables de seguimiento
            imagesProcessed = 0;
            hasErrorOccurred = false;

            StartBackgroundTask(GetImageUrlsFromSupabase());
        }

        private static IEnumerator GetImageUrlsFromSupabase()
        {
            string projectId = User.CurrentSessionInfo.project_id;

            if (string.IsNullOrEmpty(projectId))
            {
                User.Logout();
                yield break;
            }

            string url = SUPABASE_URL + "&project_id=eq." + UnityWebRequest.EscapeURL(projectId);

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", API_KEY);
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                // Envolver el JSON para que pueda ser parseado por JsonUtility
                WebProjectList webProjectList = WrapJson(jsonResult);

                // Obtener los nombres de los archivos de las imágenes
                List<string> imageFileNames = new List<string>();

                // There should be only one project in the list. Technically, there should be only one project per project_id...
                foreach (WebProject project in webProjectList.items)
                {
                    if (project.ar_images != null)
                    {
                        imageFileNames.AddRange(project.ar_images);
                    }

                    if (project.url_id != null)
                    {
                        EditorPrefs.SetString("LastAR_UrlId", project.url_id);
                    }

                    if (project.type != null)
                    {
                        ARManager aRManager = ARManager.Instance;

                        if (aRManager != null)
                        {
                            switch (project.type)
                            {
                                case "ar_with_target":
                                    aRManager.arProjectType = ARManager.ARProjectType.Image;
                                    break;
                                case "ar_world":
                                    aRManager.arProjectType = ARManager.ARProjectType.World;
                                    break;
                                default:
                                    aRManager.arProjectType = ARManager.ARProjectType.None;
                                    break;
                            }

                            EditorUtility.SetDirty(aRManager);
                        }
                    }
                }


                if (imageFileNames.Count == 0)


                {
                    LoadingState = ArTargetsLoaderState.SuccessWithNoImages;
                    yield break;
                }

                // Inicializar loadedSprites con capacidad fija
                loadedSprites = new List<Sprite>(new Sprite[imageFileNames.Count]);

                // Establecer totalImages para el seguimiento del progreso
                totalImages = imageFileNames.Count;

                // Cargar las imágenes utilizando los nombres de archivo con índices
                for (int i = 0; i < imageFileNames.Count; i++)
                {
                    string fileName = imageFileNames[i];
                    int index = i; // Capturar el índice actual
                    StartBackgroundTask(LoadImageFromUrl(fileName, index));
                }
            }
            else
            {
                string errorContent = request.downloadHandler.text;

                if (errorContent.Contains("PGRST301")) // Unauthorized
                {
                    EditorUtility.DisplayDialog("Error", "Tu sesión ha expirado. Por favor, inicia sesión de nuevo.", "OK");
                    User.Logout();
                    GUIUtility.ExitGUI();
                }
                else
                    Debug.LogError("Error al obtener las URLs de las imágenes:" +
                    "\nCódigo de respuesta: " + request.responseCode +
                    "\nError: " + request.error +
                    "\nTexto de respuesta: " + errorContent);

                LoadingState = ArTargetsLoaderState.Error;
            }
        }

        private static WebProjectList WrapJson(string json)
        {
            json = "{ \"items\": " + json + "}";
            return JsonUtility.FromJson<WebProjectList>(json);
        }

        private static IEnumerator LoadImageFromUrl(string fileName, int index)
        {
            // Construir la URL completa
            string baseUrl = "https://storage.ugame.ai/";
            string imageUrl = baseUrl + fileName;

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                while (texture == null)
                {
                    yield return null;
                }

                // Crear un Sprite a partir de la Texture2D
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f) // Pivot en el centro
                );

                // Asignar el sprite al índice correcto en la lista
                loadedSprites[index] = sprite;
            }
            else
            {
                // Si ocurre un error, establecer el estado a Error
                hasErrorOccurred = true;
                LoadingState = ArTargetsLoaderState.Error;

                Debug.LogError("Error al cargar la imagen desde URL: " + www.error +
                    "\nCódigo de respuesta: " + www.responseCode +
                    "\nURL: " + imageUrl);

                // Opcional: Salir de la corrutina ya que no necesitamos continuar
                yield break;
            }

            // Incrementar el contador de imágenes procesadas
            imagesProcessed++;

            // Verificar si todas las imágenes se han procesado
            if (imagesProcessed == totalImages)
            {
                // Si no ha ocurrido ningún error y el estado no es Error, entonces es Success
                if (!hasErrorOccurred && LoadingState != ArTargetsLoaderState.Error)
                {
                    LoadingState = ArTargetsLoaderState.Success;
                }
                // Si ha ocurrido un error, el estado ya está establecido en Error
            }
        }

        public static void StartBackgroundTask(IEnumerator update, Action end = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        end?.Invoke();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    end?.Invoke();
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}