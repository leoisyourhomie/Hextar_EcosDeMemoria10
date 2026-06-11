namespace Expiria3DSpace
{

    // Assets/Editor/WebGLBuildAndUpload.cs
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System;
    using Expiria3DSpace.UGSService.Controller;
    using System.Threading.Tasks;

    public static class WebGLBuildAndUpload
    {
        private const string BuildPath = "Builds/WebGLBuild";

        // Required file extensions
        private static readonly string[] requiredExtensionsForImageTracking = new string[]
        {
            ".data.br",
            ".framework.js.br",
            ".loader.js",
            ".wasm.br"
        };

        private static readonly string[] requiredExtensionsForWorldTracking = new string[]
{
            ".data",
            ".framework.js",
            ".loader.js",
            ".wasm"
};

        // Maximum file size (1gb)
        private const long MAX_FILE_SIZE = 256L * 1024L * 1024L;

        // Replace with your Supabase details
        private const string SupabaseUrl = "https://tpwwnbjhlnyoyeyrplkb.supabase.co"; // e.g., "https://xyzcompany.supabase.co"
        private const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRwd3duYmpobG55b3lleXJwbGtiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDE3NDk3MDgsImV4cCI6MjAxNzMyNTcwOH0.Mfr93JX7mtjsGgThB2rj_yHZByxWu0x79K4MpsVK9YQ"; // Your Supabase API key

        public static void BuildUploadAndUpdate()
        {
            // Display a progress bar
            EditorUtility.DisplayProgressBar("Build and Upload", "Starting build...", 0f);

            try
            {
                BuildAndUpload();
            }
            finally
            {
                // Clear the progress bar
                EditorUtility.ClearProgressBar();
            }
        }

        private static void BuildAndUpload()
        {
            string accessToken = User.CurrentSessionInfo.jwt;
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.LogError("Failed to authenticate.");
                return;
            }

            // Step 1: Fetch previously uploaded files
            EditorUtility.DisplayProgressBar("Build and Upload", "Fetching previous uploads...", 0.1f);
            List<string> previousUploadedFileIds = GetPreviousUploadedFiles(accessToken);
            if (previousUploadedFileIds == null)
            {
                Debug.LogError("Failed to fetch previous uploaded files.");
                return;
            }

            // Step 2: If there are previous files, confirm deletion
            if (previousUploadedFileIds.Count > 0)
            {
                bool confirmDelete = EditorUtility.DisplayDialog(
                    "Subir proyecto",
                    "Esta acción reemplazará la versión previamente subida.\r\nNo podrás deshacer esta acción.\r\n\r\n¿Estás seguro de que deseas continuar?",
                    "Sí",
                    "No"
                );

                if (!confirmDelete)
                {
                    return;
                }

                // Delete previous files
                EditorUtility.DisplayProgressBar("Exportando y subiendo", "Deleting previous uploads...", 0.2f);
                bool deletionSuccess = DeletePreviousFiles(previousUploadedFileIds, accessToken);
                if (!deletionSuccess)
                {
                    Debug.LogError("Failed to delete previous files.");
                    return;
                }

                // Update database to clear previous file IDs
                bool dbClearSuccess = UpdateDatabase(new List<FileUploadResult>(), accessToken);
                if (!dbClearSuccess)
                {
                    Debug.LogError("Failed to update database after deleting previous files.");
                    return;
                }

                Debug.Log("Previous files deleted successfully.");
            }

            if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            }
            else
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            }

            // Step 3: Build the project
            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Building project...", 0.3f);

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = BuildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogWarning("Se canceló la exportación del proyecto.");
                    return;
                }
            }
            catch (TaskCanceledException)
            {
                Debug.LogError("Se canceló la exportación del proyecto.");
                return;
            }

            // Step 4: Get the list of build files
            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Retrieving build files...", 0.5f);

            string[] buildFiles = GetExportedFiles(BuildPath);

            // Step 5: Filter and validate build files
            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Validating build files...", 0.6f);

            // Filter files to only include required extensions
            string[] filesToUpload = FilterRequiredFiles(buildFiles);

            // Validate that all required files are present
            if (!ValidateRequiredFiles(filesToUpload))
            {
                Debug.LogError("Required build files are missing.");
                return;
            }

            // Step 6: Upload files
            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Uploading files...", 0.7f);
            List<FileUploadResult> uploadedFiles = new List<FileUploadResult>();
            int totalFiles = filesToUpload.Length;
            int currentFileIndex = 0;

            foreach (string filePath in filesToUpload)
            {
                string fileName = Path.GetFileName(filePath);

                // Update progress
                float progress = 0.7f + (0.2f * currentFileIndex / totalFiles);
                EditorUtility.DisplayProgressBar("Exportando y subiendo", $"Uploading {fileName}...", progress);

                // Validate file size
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    Debug.LogError($"File {fileName} exceeds the maximum size of 256MB.");
                    return;
                }

                string uploadedFileId = UploadFile(filePath, fileName, accessToken);
                if (string.IsNullOrEmpty(uploadedFileId))
                {
                    Debug.LogError($"Failed to upload file: {fileName}");
                    return;
                }
                else
                {
                    // Debug.Log($"Uploaded file: {fileName}");
                    uploadedFiles.Add(new FileUploadResult { Name = fileName, UploadedFileId = uploadedFileId });
                }

                currentFileIndex++;
            }

            // Step 7: Update database with new file IDs
            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Updating database...", 0.9f);
            bool dbUpdateSuccess = UpdateDatabase(uploadedFiles, accessToken);
            if (!dbUpdateSuccess)
            {
                Debug.LogError("Failed to update the database.");
                return;
            }

            EditorUtility.DisplayProgressBar("Exportando y subiendo", "Process completed successfully.", 1f);

            if (EditorUtility.DisplayDialog("Subida exitosa", "El proyecto se ha subido correctamente.", "Abrir en la Web", "Cerrar"))
            {
                Application.OpenURL("https://my.expiria3d.com/" + EditorPrefs.GetString("LastAR_UrlId", ""));
            }

            GUIUtility.ExitGUI();
        }

        private static List<string> GetPreviousUploadedFiles(string accessToken)
        {
            string projectId = User.CurrentSessionInfo.project_id;

            string url = $"{SupabaseUrl}/rest/v1/web_projects?select=webgl_file_ids,url_id&project_id=eq.{projectId}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("apikey", SupabaseAnonKey);
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                request.SetRequestHeader("Content-Type", "application/json");

                // Send the request and block until done
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Wait for the request to complete
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseText = request.downloadHandler.text;
                    // Parse the response JSON array
                    string fixedJson = FixJsonArray(responseText);
                    WebProjectDataWrapper responseData = JsonUtility.FromJson<WebProjectDataWrapper>(fixedJson);

                    if (responseData != null && responseData.items != null && responseData.items.Count > 0)
                    {
                        var fileIds = responseData.items[0].webgl_file_ids;
                        EditorPrefs.SetString("LastAR_UrlId", responseData.items[0].url_id);

                        if (fileIds != null)
                        {
                            return new List<string>(fileIds);
                        }
                    }
                    return new List<string>();
                }
                else
                {
                    string errorContent = request.downloadHandler.text;

                    if (errorContent.Contains("PGRST301")) // Unauthorized
                    {
                        EditorUtility.DisplayDialog("Error", "Tu sesión ha expirado. Por favor, inicia sesión de nuevo.", "OK");
                        User.Logout();
                    }
                    else
                        Debug.LogError($"Error fetching previous uploaded files: {request.error} - {errorContent}");

                    GUIUtility.ExitGUI();
                    return null;
                }
            }
        }

        private static string FixJsonArray(string json)
        {
            return "{\"items\":" + json + "}";
        }

        private static bool DeletePreviousFiles(List<string> uploadedFileIds, string accessToken)
        {
            foreach (var uploadedFileId in uploadedFileIds)
            {
                bool deletionSuccess = DeleteFile(uploadedFileId, accessToken);
                if (!deletionSuccess)
                {
                    Debug.LogError($"Failed to delete file: {uploadedFileId}");
                    return false;
                }
                // else
                // {
                //     Debug.Log($"Deleted file: {uploadedFileId}");
                // }
            }
            return true;
        }

        private static bool DeleteFile(string uploadedFileId, string accessToken)
        {
            string fileKey = Uri.EscapeDataString(uploadedFileId);
            string deleteUrl = $"https://storage.ugame.ai/{fileKey}";

            using (UnityWebRequest request = UnityWebRequest.Delete(deleteUrl))
            {
                request.SetRequestHeader("X-Auth-Key", accessToken);

                // Send the request and block until done
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Wait for the request to complete
                }

                if (request.responseCode == 204)
                {
                    // Deletion successful
                    return true;
                }
                else if (request.responseCode == 401)
                {
                    EditorUtility.DisplayDialog("Error", "Tu sesión ha expirado. Por favor, inicia sesión de nuevo.", "OK");
                    User.Logout();
                    GUIUtility.ExitGUI();
                    return false;
                }
                else if (request.responseCode == 404)
                {
                    Debug.LogWarning($"File \"{uploadedFileId}\" not found.");
                    // Consider it successful since the file doesn't exist
                    return true;
                }
                else if (request.responseCode == 500)
                {
                    // This may happen if the user cancels the upload. Consider it successful.                    
                    // Debug.LogWarning($"The file does not exist \"{uploadedFileId}\".");
                    return true;
                }
                else
                {
                    Debug.LogError($"Error deleting file \"{uploadedFileId}\": {request.error} - {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        private static string[] GetExportedFiles(string buildPath)
        {
            if (Directory.Exists(buildPath))
            {
                return Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories);
            }
            return new string[0];
        }

        private static string[] FilterRequiredFiles(string[] files)
        {
            return files.Where(file =>
            {
                string fileName = Path.GetFileName(file);
                if(ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                    return requiredExtensionsForImageTracking.Any(ext => fileName.EndsWith(ext));
                else 
                    return requiredExtensionsForWorldTracking.Any(ext => fileName.EndsWith(ext));
            }).ToArray();
        }

        private static bool ValidateRequiredFiles(string[] files)
        {
            HashSet<string> fileSet = new HashSet<string>(files.Select(f => Path.GetFileName(f)));
            if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                return requiredExtensionsForImageTracking.All(ext => fileSet.Any(name => name.EndsWith(ext)));
            else
                return requiredExtensionsForWorldTracking.All(ext => fileSet.Any(name => name.EndsWith(ext)));
        }


        private static string UploadFile(string filePath, string fileName, string accessToken)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            string fileKey = Uri.EscapeDataString(fileName);
            string presignedUrl = $"https://storage.ugame.ai/{fileKey}";

            string uploadUrl = null;
            string fileUrl = null;
            string fileId = null;

            // Determinar el Content-Type basado en la extensión del archivo
            string contentType;

            if (fileName.EndsWith(".data.br") || fileName.EndsWith(".data"))
            {
                contentType = "application/octet-stream";
            }
            else if (fileName.EndsWith(".wasm.br") || fileName.EndsWith(".wasm"))
            {
                contentType = "application/wasm";
            }
            else if (fileName.EndsWith(".framework.js.br") || fileName.EndsWith(".loader.js")
                || fileName.EndsWith(".framework.js"))
            {
                contentType = "application/javascript";
            }
            else
            {
                contentType = "application/octet-stream";
            }

            // PASO 1: Obtener la URL pre-firmada
            using (UnityWebRequest request = new UnityWebRequest(presignedUrl, UnityWebRequest.kHttpVerbPOST))
            {
                // Puedes enviar un cuerpo vacío o los datos necesarios según tu API
                byte[] bodyRaw = new byte[0];
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("X-Use-Presigned-Url", "true");
                request.SetRequestHeader("X-Security-Type", "public");
                request.SetRequestHeader("X-Auth-Key", accessToken);
                request.SetRequestHeader("Content-Type", contentType); // Usar el Content-Type determinado
                request.SetRequestHeader("X-File-Size", fileData.Length.ToString());

                // Enviar la solicitud y esperar hasta que se complete
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Esperar a que la solicitud se complete
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;

                    try
                    {
                        // Parsear la respuesta JSON para obtener uploadUrl, fileUrl y fileId
                        var responseData = JsonUtility.FromJson<PresignedUrlResponse>(responseText);

                        uploadUrl = responseData.uploadUrl;
                        fileUrl = responseData.fileUrl;
                        fileId = responseData.fileId;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error al parsear la respuesta JSON: " + ex.Message);
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error obteniendo la URL pre-firmada para el archivo \"{fileName}\". Error: {request.error} - {request.downloadHandler.text}");
                    return null;
                }
            }

            // PASO 2: Subir el archivo a la URL pre-firmada
            bool uploadSucceeded = false;
            using (UnityWebRequest request = new UnityWebRequest(uploadUrl, UnityWebRequest.kHttpVerbPUT))
            {
                request.uploadHandler = new UploadHandlerRaw(fileData);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", contentType); // Usar el Content-Type determinado

                // Enviar la solicitud y esperar hasta que se complete
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Puedes implementar aquí la lógica para mostrar el progreso si lo deseas
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Subida exitosa
                    uploadSucceeded = true;
                }
                else
                {
                    Debug.LogError($"Error subiendo el archivo \"{fileName}\". Error: {request.error} - {request.downloadHandler.text}");
                    uploadSucceeded = false;
                }
            }

            // PASO 3: Confirmar o cancelar la subida según el resultado
            if (uploadSucceeded)
            {
                // Confirmar la subida
                bool confirmSuccess = ConfirmUpload(fileId, accessToken);

                if (confirmSuccess)
                {
                    return fileUrl.Replace("https://storage.ugame.ai/", "").Replace("https://storage.expiria3d.com/", "");
                }
                else
                {
                    // Si la confirmación falla, intenta cancelar la subida
                    CancelUpload(fileId, accessToken);
                    return null;
                }
            }
            else
            {
                // Si la subida falla, intenta cancelar la subida
                CancelUpload(fileId, accessToken);
                return null;
            }
        }


        // Método para confirmar la subida del archivo
        private static bool ConfirmUpload(string fileId, string accessToken)
        {
            string url = "https://storage.ugame.ai/confirm-upload";

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                string jsonBody = $"{{\"fileId\":\"{fileId}\"}}";
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Auth-Key", accessToken);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Esperar a que la solicitud se complete
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Confirmación exitosa
                    return true;
                }
                else
                {
                    Debug.LogError($"Error confirmando la subida para fileId \"{fileId}\". Error: {request.error} - {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        // Método para cancelar la subida del archivo
        private static void CancelUpload(string fileId, string accessToken)
        {
            string url = "https://storage.ugame.ai/cancel-upload";

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                string jsonBody = $"{{\"fileId\":\"{fileId}\"}}";
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Auth-Key", accessToken);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Esperar a que la solicitud se complete
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Subida cancelada para fileId \"{fileId}\".");
                }
                else
                {
                    Debug.LogError($"Error cancelando la subida para fileId \"{fileId}\". Error: {request.error} - {request.downloadHandler.text}");
                }
            }
        }

        private static bool UpdateDatabase(List<FileUploadResult> uploadedFiles, string accessToken)
        {
            // Include the on_conflict parameter in the URL
            string updateUrl = $"{SupabaseUrl}/rest/v1/web_projects?on_conflict=project_id&project_id=eq.{User.CurrentSessionInfo.project_id}";

            // Create the payload
            var payload = new WebProjectUpdate
            {
                project_id = User.CurrentSessionInfo.project_id,
                webgl_file_ids = uploadedFiles.Select(f => f.UploadedFileId).ToArray(),
            };

            // Convert payload to JSON
            string jsonData = JsonUtility.ToJson(payload);

            using (UnityWebRequest request = new UnityWebRequest(updateUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", SupabaseAnonKey);
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                request.SetRequestHeader("Prefer", "resolution=merge-duplicates");

                // Send the request and block until done
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    // Wait for the request to complete
                }

                if (request.result == UnityWebRequest.Result.Success || request.responseCode == 201 || request.responseCode == 204)
                {
                    return true;
                }
                else
                {
                    string errorContent = request.downloadHandler.text;

                    if (errorContent.Contains("PGRST301")) // Unauthorized
                    {
                        EditorUtility.DisplayDialog("Error", "Tu sesión ha expirado. Por favor, inicia sesión de nuevo.", "OK");
                        User.Logout();
                    }
                    else
                        Debug.LogError($"Error updating database: {request.error} - {request.downloadHandler.text}");

                    GUIUtility.ExitGUI();
                    return false;
                }
            }
        }


        [Serializable]
        private class FileUploadResult
        {
            public string Name;
            public string UploadedFileId;
        }

        [Serializable]
        private class WebProjectData
        {
            public string[] webgl_file_ids;
            public string url_id;
        }

        [Serializable]
        private class WebProjectUpdate
        {
            public string project_id;
            public string[] webgl_file_ids;
        }

        [Serializable]
        private class WebProjectDataWrapper
        {
            public List<WebProjectData> items;
        }

        [Serializable]
        public class PresignedUrlResponse
        {
            public string uploadUrl;
            public string fileUrl;
            public string fileId;
        }
    }
}