namespace Expiria3DSpace
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Expiria3DSpace.UGSService.Controller;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;
    using UnityEngine.Networking;

    [InitializeOnLoad]
    public class CheckUserConnection
    {
        private const float interval = 5 * 60; // 5 minutes
        private static float lastUpdateTime;
        private const float updateRate = 5f; //5 seconds

        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static CheckUserConnection()
        {
            double _lastTime = EditorPrefs.GetFloat("ugs_last_time", 0);

            //The last time never should be greater than the time Unity is running.
            //If it is greater, it means that it is a new session
            if (_lastTime > EditorApplication.timeSinceStartup)
            {
                _lastTime = EditorApplication.timeSinceStartup;
                EditorPrefs.SetFloat("ugs_last_time", (float)_lastTime);
            }

            lastUpdateTime = (float)EditorApplication.timeSinceStartup;

            CompilationPipeline.compilationStarted -= CompilationStarted;
            CompilationPipeline.compilationStarted += CompilationStarted;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void CompilationStarted(object obj)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup - lastUpdateTime >= updateRate)
            {
                lastUpdateTime = (float)EditorApplication.timeSinceStartup;

                double currentTime = EditorApplication.timeSinceStartup;
                float lastTime = EditorPrefs.GetFloat("ugs_last_time", 0); // Retrieve lastTime from EditorPrefs

                if (currentTime - lastTime >= interval)
                {
                    // Reset the timer
                    EditorPrefs.SetFloat("ugs_last_time", (float)currentTime); // Store the new lastTime in EditorPrefs

                    if (!User.IsLoggedIn)
                    {
                        return;
                    }

                    CheckSession();
                }
            }
        }

        private static async void CheckSession()
        {
            // Create the form
            WWWForm form = new WWWForm();
            form.AddField("data", "check_session¦" + User.CurrentSessionInfo.jwt + "¦" + User.CurrentSessionInfo.team_id + "¦" + User.CurrentSessionInfo.project_id + "¦" + EditorPrefs.GetInt("ugs_machine_validation_error_count", 0));

            // Create the request
            using (UnityWebRequest www = UnityWebRequest.Post(User.ServiceURL, form))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();

                // Wait until the UnityWebRequest is done
                try
                {
                    await operation.AsTask(cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    //Debug.Log("<color=yellow>Expiria3D: </color>Background Task was cancelled: " + e.Message);
                    return;
                }

                if (www.error == "Request aborted")
                {
                    CountError();
                }

                string response = www.downloadHandler?.text;

                if (string.IsNullOrEmpty(response))
                {
                    return;
                }

                switch (response)
                {
                    case "CORRECT":
                        ResetErrorCount();
                        return;
                    case "NOT_PART_OF_PROJECT_OR_DOES_NOT_EXIST":
                        User.Logout();
                        CountError();

                        EditorUtility.DisplayDialog("Error de proyecto", "El proyecto actual no existe en la plataforma de Expiria3D o no eres parte de él. Debes iniciar sesión nuevamente en tu cuenta de Expiria3D.", "Ok");

                        break;
                    case "NO_PLAN_OR_NOT_IN_TEAM":
                        User.Logout();
                        CountError();

                        EditorUtility.DisplayDialog("Error de plan", "El Team seleccionado no tiene un plan activo o no perteneces a este Team. Debes iniciar sesión nuevamente en tu cuenta de Expiria3D.", "Ok");

                        break;
                    case "WRONG_JWT":
                        User.Logout();
                        CountError();
                        //We should not reset the error count here, because the JWT is wrong

                        EditorUtility.DisplayDialog("Error de sesión", "La sesión actual no es válida o ha expirado. Debes iniciar sesión nuevamente en tu cuenta de Expiria3D.", "Ok");

                        break;
                    default:
                        CountError();
                        break;
                }
            }
        }

        private static void ResetErrorCount()
        {
            EditorPrefs.SetInt("ugs_machine_validation_error_count", 0);
        }

        private static void CountError()
        {
            int current = EditorPrefs.GetInt("ugs_machine_validation_error_count", 0);
            EditorPrefs.SetInt("ugs_machine_validation_error_count", current + 1);
        }
    }

    public static class UnityWebRequestExtensions
    {
        public static Task<AsyncOperation> AsTask(this AsyncOperation asyncOperation, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<AsyncOperation>();

            asyncOperation.completed += obj => { tcs.TrySetResult(obj); };
            cancellationToken.Register(() =>
            {
                asyncOperation.completed -= obj => { tcs.TrySetResult(obj); };
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }
    }
}