namespace Expiria3DSpace
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections;
    using System;
    using System.IO;
    using UnityEngine.Networking;
    using System.Net;

    public class DownloadManager
    {
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

        public static void DeleteTemPackages()
        {
            if (AssetDatabase.IsValidFolder("Assets/Expiria3D/Temp_Packages"))
            {
                FileUtil.DeleteFileOrDirectory("Assets/Expiria3D/Temp_Packages");
                AssetDatabase.DeleteAsset("Assets/Expiria3D/Temp_Packages.meta");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static string GetTextInWebURL(string url_of_txt_file)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    return client.DownloadString(url_of_txt_file);
                }
                catch
                {
                    return "";
                }
            }
        }

        public static IEnumerator DownloadFile(string fileURL, bool interactive = true)
        {
            string finalFileName = "UGame-Studio-Package-ID-" + UnityEngine.Random.Range(0, int.MaxValue).ToString() + ".unitypackage";

            var uwr = new UnityWebRequest(fileURL, UnityWebRequest.kHttpVerbGET);
            string path = Path.Combine(Application.dataPath + "/Expiria3D/Temp_Packages", finalFileName);
            uwr.downloadHandler = new DownloadHandlerFile(path);

            uwr.SendWebRequest();

            while (!uwr.isDone)
            {
                EditorUtility.DisplayProgressBar("Downloading...", Mathf.RoundToInt(uwr.downloadProgress * 100) + "%", uwr.downloadProgress);
            }
            EditorUtility.ClearProgressBar();
            yield return null;

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                EditorUtility.DisplayDialog("Oops, something went wrong :(", "Please try again in a minute. If the error continues, contact with the Expiria3D support. \n\nThis was the error: " + uwr.error, "Ok");
                GUIUtility.ExitGUI();
            }
            else
            {
                AssetDatabase.importPackageCancelled += ImportCancelled;
                AssetDatabase.importPackageFailed += ImportCallBackFailed;
                AssetDatabase.importPackageCompleted += ImportCompleted;
                AssetDatabase.ImportPackage(Application.dataPath + "/Expiria3D/Temp_Packages/" + finalFileName, interactive);
                AssetDatabase.Refresh();
            }
        }

        static void ImportCompleted(string packageName)
        {
            DeleteTemPackages();

            AssetDatabase.importPackageCompleted -= ImportCompleted;

            //This is only used for PluginsForExpiria3DEditor.cs
            EditorPrefs.SetBool("_resolved", false);
        }

        static void ImportCancelled(string packageName)
        {
            DeleteTemPackages();

            AssetDatabase.importPackageCancelled -= ImportCancelled;
        }

        static void ImportCallBackFailed(string packageName, string _error)
        {
            EditorUtility.DisplayDialog("Import failed", _error, "Ok");

            DeleteTemPackages();

            AssetDatabase.importPackageFailed -= ImportCallBackFailed;
            GUIUtility.ExitGUI();
        }
    }
}
