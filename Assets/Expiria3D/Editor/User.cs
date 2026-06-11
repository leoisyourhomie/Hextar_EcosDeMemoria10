namespace Expiria3DSpace
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    namespace UGSService.Controller
    {
        public static class User
        {
            public const string ServiceURL = "https://unity.ugame.workers.dev/";

            [System.Serializable]
            public class SessionInfo
            {
                public string jwt;
                public string team_id;
                public string project_id;
                public Profile profile = new Profile();
                public Team team = new Team();
                public Project project = new Project();
            }

            [System.Serializable]
            public class Profile
            {
                public string last_name;
                public string avatar_url;
                public string first_name;

                public string FullName
                {
                    get
                    {
                        return first_name + " " + last_name;
                    }
                }
            }

            [System.Serializable]
            public class Team
            {
                public string name;
                public string avatar_url;
            }

            [System.Serializable]
            public class Project
            {
                public string name;
                public string avatar_url;
            }

            static SessionInfo currentSessionInfo = null;
            static string currentSessionInfoJson = null;
            public static SessionInfo CurrentSessionInfo
            {
                get
                {
                    if (currentSessionInfo == null)
                    {
                        if (string.IsNullOrEmpty(CurrentSessionInfoJson))
                        {
                            return null;
                        }

                        currentSessionInfo = JsonUtility.FromJson<SessionInfo>(CurrentSessionInfoJson);
                    }

                    return currentSessionInfo;
                }
            }

            public static string CurrentSessionInfoJson
            {
                get
                {
                    if (string.IsNullOrEmpty(currentSessionInfoJson))
                    {
                        currentSessionInfoJson = EditorPrefs.GetString("ugame_user_session_data", "");
                    }

                    return currentSessionInfoJson;
                }
                set
                {
                    if (value.Contains("jwt") && value.Contains("team_id") && value.Contains("project_id") && value.Contains("profile") && value.Contains("team") && value.Contains("project"))
                    {
                        EditorPrefs.SetString("ugame_user_session_data", value);
                        EditorPrefs.SetString("userLicense", "003");
                        currentSessionInfoJson = value;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Invalid session data", "Ok");
                    }
                }
            }

            public static void Logout()
            {
                currentSessionInfo = null;
                currentSessionInfoJson = null;
                EditorPrefs.DeleteKey("ugame_user_session_data");
                EditorPrefs.DeleteKey("userLicense");
                UGSServiceManager.UserValidation.login_code = null;
            }

            public static bool IsLoggedIn
            {
                get
                {
                    return !string.IsNullOrEmpty(CurrentSessionInfoJson) && CurrentSessionInfoJson.Contains("jwt");
                }
            }

            public static string GetLoginCode(string email)
            {
                if (string.IsNullOrEmpty(email))
                {
                    EditorUtility.DisplayDialog("Error", "Ingresa tu email", "Ok");
                    return null;
                }

                WWWForm form = new WWWForm();
                form.AddField("data", "request_code¦" + email);

                UnityWebRequest www = UnityWebRequest.Post(User.ServiceURL, form);
                // Esto bloqueará la UI hasta que se complete la solicitud
                www.SendWebRequest();

                while (!www.isDone)
                {
                }

                if (www.responseCode == 200)
                {
                    //return the response body
                    return www.downloadHandler.text;
                }
                else
                {
                    if (www.responseCode == 404)
                    {
                        EditorUtility.DisplayDialog("Error","El usuario no fue encontrado", "Ok");
                    }
                    else if (www.responseCode == 403)
                    {
                        EditorUtility.DisplayDialog("Error","Bad Request", "Ok");
                    }
                    else if (www.responseCode == 500)
                    {
                        EditorUtility.DisplayDialog("Error", "Error al buscar el usuario", "Ok");
                    }
                    else
                    {
                        Debug.Log("<color=yellow>Expiria3D: </color>" + www.error);
                    }

                    return null;
                }
            }

            public static void ConfirmLogin(string email, string code)
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
                {
                    EditorUtility.DisplayDialog("Error", "Something went wrong. Please close Unity and open it again.", "Ok");
                    return;
                }

                WWWForm form = new WWWForm();
                form.AddField("data", "check_code¦" + email + "¦" + code);

                UnityWebRequest www = UnityWebRequest.Post(User.ServiceURL, form);

                www.SendWebRequest();

                while (!www.isDone)
                {
                }

                if (www.responseCode == 200)
                {
                    CurrentSessionInfoJson = www.downloadHandler.text;
                    currentSessionInfo = JsonUtility.FromJson<SessionInfo>(CurrentSessionInfoJson);
                }
                else
                {
                    if (www.responseCode == 401)
                    {
                        EditorUtility.DisplayDialog("Error", "El código cambió o no iniciaste sesión desde el mismo dispositivo.", "Ok");
                    }
                    else if (www.responseCode == 404)
                    {
                        EditorUtility.DisplayDialog("Error", "El usuario no fue encontrado", "Ok");
                    }
                    else if (www.responseCode == 403)
                    {
                        EditorUtility.DisplayDialog("Error", "Bad Request", "Ok");
                    }
                    else if (www.responseCode == 500)
                    {
                        EditorUtility.DisplayDialog("Error", "Error al buscar el usuario", "Ok");
                    }
                    else
                    {
                        Debug.Log("<color=yellow>Expiria3D: </color>" + "[" + www.responseCode + "] " + www.error);
                    }
                }
            }
        }
    }
}
