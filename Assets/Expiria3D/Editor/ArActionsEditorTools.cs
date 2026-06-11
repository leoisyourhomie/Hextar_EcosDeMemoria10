namespace Expiria3DSpace
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class ArActionsEditorTools
    {
        public static GUIStyle BigButtonStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.button) { wordWrap = true, padding = new RectOffset(10, 10, 10, 10), fontSize = 15 };
            }
        }

        public static List<(Type, string, string)> actionsInfo = new List<(Type, string, string)>
        {
            (typeof(ArAction_OpenUrl), "Abrir una página web al hacer clic", "ar_web.png"),
            (typeof(ArAction_OpenWhatsApp), "Abrir WhatsApp al hacer clic", "ar_wpp.png"),
            (typeof(ArAction_MakeCall), "Hacer una llamada al hacer clic", "ar_phone.png"),
            (typeof(ArAction_SendEmail), "Enviar correo electrónico al hacer clic", "ar_email.png"),
            (typeof(ArAction_SendTextMessage), "Enviar un mensaje de texto (SMS) al hacer clic", "ar_sms.png"),
            (typeof(ArAction_EnableOrDisableObjects), "Activar o desactivar objetos", "ar_settings.png"),
            (typeof(ArAction_PlaySound), "Reproducir un sonido", "ar_sound.png"),
            (typeof(ArAction_RotateObject), "Rotar objeto ", "ar_rotate.png"),
            (typeof(ArAction_MoveObject), "Mover objeto", "ar_move.png"),
            (typeof(ArAction_PlayAnimation), "Reproducir animación del modelo", "ar_animation.png"),
            (typeof(ArAction_ChangeVideoURL), "Cambiar URL del Reproductor de Video", "ar_video.png"),
        };

        //cache the icons
        private static Dictionary<string, Texture> icons = new Dictionary<string, Texture>();

        public static Texture GetIcon(ArActionBase action)
        {
            Type actionType = action.GetType();
            var actionInfo = actionsInfo.Find(x => x.Item1 == actionType);
            if (!icons.ContainsKey(actionInfo.Item3))
            {
                icons.Add(actionInfo.Item3, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/" + actionInfo.Item3));
            }
            Texture icon = icons[actionInfo.Item3];
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/ar_web.png");
            }

            return icon;
        }

        public static Texture GetIcon(Type actionType)
        {
            var actionInfo = actionsInfo.Find(x => x.Item1 == actionType);
            if (!icons.ContainsKey(actionInfo.Item3))
            {
                icons.Add(actionInfo.Item3, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/" + actionInfo.Item3));
            }
            Texture icon = icons[actionInfo.Item3];
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Expiria3D/EditorIcons/ArActions/ar_web.png");
            }
            return icon;
        }

        public static string GetName(ArActionBase action)
        {
            Type actionType = action.GetType();
            var actionInfo = actionsInfo.Find(x => x.Item1 == actionType);
            return actionInfo.Item2;
        }

        public static string GetName(Type actionType)
        {
            var actionInfo = actionsInfo.Find(x => x.Item1 == actionType);
            return actionInfo.Item2;
        }

        public static void WarningWithBigFont(string message)
        {
            EditorGUILayout.BeginHorizontal(new GUIStyle("helpbox") { padding = new RectOffset(10, 10, 10, 10) });

            GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon"), GUILayout.Width(28), GUILayout.Height(28));
            GUIStyle style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 14, wordWrap = true };
            EditorGUILayout.LabelField(message, style);
            EditorGUILayout.EndHorizontal();
        }

        public static void ErrorWithBigFont(string message)
        {
            EditorGUILayout.BeginHorizontal(new GUIStyle("helpbox") { padding = new RectOffset(10, 10, 10, 10) });

            GUILayout.Label(EditorGUIUtility.IconContent("console.erroricon"), GUILayout.Width(28), GUILayout.Height(28));
            GUIStyle style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 14, wordWrap = true };
            EditorGUILayout.LabelField(message, style);
            EditorGUILayout.EndHorizontal();
        }
    }

}