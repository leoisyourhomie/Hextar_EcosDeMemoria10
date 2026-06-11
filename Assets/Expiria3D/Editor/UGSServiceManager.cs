namespace Expiria3DSpace
{

    using System;
    using System.Collections.Generic;
    using Expiria3DSpace.UGSService.Controller;
    using UnityEditor;
    using UnityEngine;

    public class UGSServiceManager : Expiria3DManager
    {
        static Texture logoIcon;

        public class UserValidation
        {
            public static string user_email = "";
            public static string login_code = "";
            static GUIStyle style = null;
            const string UNITY_UGAME_LOGIN_PAGE = "https://expiria3d.com/login?unity_code=";

            static void BeginCenter()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            static void EndCenter()
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            public static bool ValidateUser(bool showUserInfo)
            {
                if (logoIcon == null)
                {
                    logoIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/logo_expiria_sm.png");
                }

                if (style == null)
                {
                    style = new GUIStyle();
                    style.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Expiria3D/EditorIcons/bg.png");
                    style.margin = new RectOffset(15, 15, 15, 15);
                    style.border = new RectOffset(3, 3, 3, 3);
                    style.overflow = new RectOffset(8, 8, 8, 8);
                }

                //This groupbox finishes before every return...
                if (showUserInfo)
                    GUI.color = new Color(1, 1, 1, 0.9f);

                EditorGUILayout.BeginVertical((showUserInfo) ? style : new GUIStyle(EditorStyles.inspectorDefaultMargins) { padding = new RectOffset(0, 0, 0, 0) });

                if (showUserInfo)
                    GUI.color = Color.white;

                if (User.IsLoggedIn) //if it is loged in
                {
                    if (showUserInfo)
                    {
                        BeginCenter();
                        GUILayout.Label((logoIcon), GUILayout.Height(50), GUILayout.Width(180));
                        EndCenter();

                        GUI.backgroundColor = new Color(1, 1, 1, 0.6f);

                        EditorGUILayout.BeginHorizontal("groupbox");

                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.LabelField(
    "Usuario"
    , EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(User.CurrentSessionInfo.profile.FullName);

                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button(
                             "Salir"))
                        {
                            User.Logout();
                            GUIUtility.ExitGUI();
                            return false;
                        }

                        EditorGUILayout.EndHorizontal();

                        GUI.backgroundColor = new Color(1, 1, 1, 0.6f);
                        EditorGUILayout.BeginVertical("groupbox");
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.LabelField(
                            "Proyecto actual"
                            , EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.LabelField("Equipo: " + User.CurrentSessionInfo.team.name);
                                EditorGUILayout.LabelField("Proyecto: " + User.CurrentSessionInfo.project.name);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical();
                    return true;
                }
                else
                {
                    EditorGUILayout.BeginVertical(style);
                    {
                        EditorGUILayout.BeginVertical();

                        BeginCenter();
                        GUILayout.Label((logoIcon), GUILayout.Height(50), GUILayout.Width(180));
                        EndCenter();

                        EditorGUIUtility.labelWidth = 40;

                        BeginCenter();
                        GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
                        EditorGUILayout.BeginVertical("groupbox", GUILayout.Width(10));

                        if (string.IsNullOrEmpty(login_code))
                        {
                            GUI.backgroundColor = Color.white;
                            EditorGUILayout.LabelField(
                               "Ingresa tu email para continuar"
                                , EditorStyles.boldLabel);

                            user_email = EditorGUILayout.TextField("Email ", user_email, GUILayout.Width(250));

                            GUILayout.Space(5);

                            void CheckEmail()
                            {
                                UGSNetworkValidator.ConnectionState connectionState = UGSNetworkValidator.ValidateConnectionToExpiria3D();

                                if (connectionState == UGSNetworkValidator.ConnectionState.Successful)
                                {
                                    login_code = User.GetLoginCode(UserValidation.user_email);
                                    if (!string.IsNullOrEmpty(login_code))
                                    {
                                        Application.OpenURL(UNITY_UGAME_LOGIN_PAGE + login_code);
                                    }
                                }
                                else if (connectionState == UGSNetworkValidator.ConnectionState.NoInternetConnection)
                                {
                                    EditorUtility.DisplayDialog("Error",
                                          "No tienes conexión a internet."
                                          ,
                                          "Ok");
                                    GUIUtility.ExitGUI();
                                }
                                else if (connectionState == UGSNetworkValidator.ConnectionState.CannotConnectWithExpiria3D)
                                {
                                    EditorUtility.DisplayDialog("Error",
                                          "Tienes conexión a Internet, pero la conexión a nuestros servidores no es posible en este momento, vuelve a intentarlo en unos minutos o ponte en contacto con el soporte: support@expiria3d.com"
                                          , "Ok");
                                    GUIUtility.ExitGUI();
                                }
                            }

                            //if press enter, check email
                            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                            {
                                CheckEmail();
                            }

                            if (GUILayout.Button(
                                "Continuar"
                                , GUILayout.Width(250), GUILayout.Height(30)))
                            {
                                CheckEmail();
                            }

                            EditorGUILayout.Space();

                            if (GUILayout.Button(
                                "Crear Nueva Cuenta"
                                , EditorStyles.miniLabel))
                            {
                                Application.OpenURL("https://expiria3d.com/signup");
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Sigue los siguientes pasos:\r\n\r\n1. Abre el enlace en tu navegador e inicia sesión.\r\n2. Selecciona tu equipo y un proyecto.\r\n3. Vuelve aquí y presiona el botón de 'Confirmar'."
                                , new GUIStyle(EditorStyles.boldLabel) { wordWrap = true });

                            GUILayout.Space(5);

                            EditorGUILayout.SelectableLabel(UNITY_UGAME_LOGIN_PAGE + login_code, EditorStyles.wordWrappedLabel, GUILayout.Width(310));
                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            //copy to clipboard button with icon
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("Clipboard"), "Copy"), GUILayout.Width(23), GUILayout.Height(25)))
                            {
                                EditorGUIUtility.systemCopyBuffer = UNITY_UGAME_LOGIN_PAGE + login_code;
                                //Show a notification in the window
                                EditorWindow.focusedWindow.ShowNotification(new GUIContent("Copiado"));

                            }

                            //open in browser button with icon
                            if (GUILayout.Button(new GUIContent("Abrir en el navegador", EditorGUIUtility.FindTexture("BuildSettings.Web"), "Abrir"), GUILayout.Height(25)))
                            {
                                Application.OpenURL(UNITY_UGAME_LOGIN_PAGE + login_code);
                            }


                            if (GUILayout.Button(new GUIContent("Cancelar", EditorGUIUtility.IconContent("P4_DeletedLocal@2x").image, "Cancel"), GUILayout.Width(85), GUILayout.Height(25)))
                            {
                                login_code = "";
                            }

                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(10);

                            if (GUILayout.Button(new GUIContent("Confirmar", EditorGUIUtility.IconContent("P4_CheckOutRemote@2x").image, "Confirm"), GUILayout.Height(30)))
                            {
                                User.ConfirmLogin(user_email, login_code);

                                ArTargetsLoader.LoadImagesAndType();
                            }
                        }


                        EditorGUILayout.EndVertical();
                        EndCenter();

                        EditorGUILayout.EndVertical();

                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndVertical();
                    return false;
                }

            }
        }

    }

}
