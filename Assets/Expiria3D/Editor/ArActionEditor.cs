using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Expiria3DSpace
{
    [CustomEditor(typeof(ArAction))]
    public class ArActionEditor : Editor
    {

        ArAction script = null;

        void OnEnable()
        {
            script = (ArAction)target;
        }

        public override void OnInspectorGUI()
        {
            // Obtiene el componente objetivo
            script = (ArAction)target;

            if (script.actionData == null)
            {
                //remove the script
                DestroyImmediate(script);
                Debug.LogError("The actionData is null. The script has been removed.");
                return;
            }

            EditorGUILayout.BeginVertical("groupbox");
            {
                DrawTitle(script.actionData);

                GUILayout.Space(10);

                script.delay = EditorGUILayout.FloatField("Esperar (segundos)", script.delay);
                //Here should be the code to draw the editor for the action
                DrawActionProperties(script.actionData);

                GUILayout.Space(10);

                // Campo de retardo

                if (script.actionData.StartWith == ArActionBase.StartWithEnum.OnClick)
                {
                    CheckErrorsForClick(script.gameObject);
                }

            }
            EditorGUILayout.EndVertical();

            EditorUtility.SetDirty(script);
            Undo.RecordObject(script, "Undo ArAction");
        }

        void DrawTitle(ArActionBase actionData)
        {
            Texture openUrlIcon = ArActionsEditorTools.GetIcon(actionData);

            EditorGUILayout.BeginHorizontal();
            {
                //draw icon
                GUILayout.Label(openUrlIcon, GUILayout.Width(30), GUILayout.Height(30));

                //flexible vertical
                EditorGUILayout.BeginVertical(GUILayout.Height(30));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(ArActionsEditorTools.GetName(actionData), new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 15, wordWrap = true });
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionProperties(ArActionBase actionData)
        {
            if (actionData is ArAction_OpenUrl)
            {
                DrawOpenUrlAction((ArAction_OpenUrl)actionData);
            }
            else if (actionData is ArAction_PlaySound)
            {
                DrawPlaySoundAction((ArAction_PlaySound)actionData);
            }
            else if (actionData is ArAction_RotateObject)
            {
                DrawRotateObjectAction((ArAction_RotateObject)actionData);
            }
            else if (actionData is ArAction_MoveObject)
            {
                DrawMoveObjectAction((ArAction_MoveObject)actionData);
            }
            else if (actionData is ArAction_PlayAnimation)
            {
                DrawPlayAnimationAction((ArAction_PlayAnimation)actionData);
            }
            else if (actionData is ArAction_MakeCall)
            {
                DrawMakeCallAction((ArAction_MakeCall)actionData);
            }
            else if (actionData is ArAction_SendTextMessage)
            {
                DrawSendTextMessageAction((ArAction_SendTextMessage)actionData);
            }
            else if (actionData is ArAction_OpenWhatsApp)
            {
                DrawOpenWhatsAppAction((ArAction_OpenWhatsApp)actionData);
            }
            else if (actionData is ArAction_SendEmail)
            {
                DrawSendEmailAction((ArAction_SendEmail)actionData);
            }
            else if (actionData is ArAction_EnableOrDisableObjects)
            {
                DrawEnableOrDisableObjectsAction((ArAction_EnableOrDisableObjects)actionData);
            }
            else if (actionData is ArAction_ChangeVideoURL)
            {
                DrawChangeVideoUrlAction((ArAction_ChangeVideoURL)actionData);
            }
        }

        private void DrawChangeVideoUrlAction(ArAction_ChangeVideoURL action)
        {
            action.videoPlayer = (PlayVideoFromURL)EditorGUILayout.ObjectField("Reproductor de Video", action.videoPlayer, typeof(PlayVideoFromURL), true);
            action.videoURL = EditorGUILayout.TextField("URL del Video", action.videoURL);
            action.videoURL = action.videoURL.Trim();
            action.playAutomatically = EditorGUILayout.Toggle("Reproducir Automáticamente", action.playAutomatically);
        }

        private void DrawEnableOrDisableObjectsAction(ArAction_EnableOrDisableObjects action)
        {
            Texture removeIcon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
            EditorGUILayout.BeginVertical("groupbox");
            {
                foreach (var obj in action.objectActions)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 50;
                        obj.targetObject = (GameObject)EditorGUILayout.ObjectField("Objeto", obj.targetObject, typeof(GameObject), true);
                        EditorGUIUtility.labelWidth = 0;

                        int actionType = (int)obj.action;
                        actionType = EditorGUILayout.Popup(actionType, new string[] { "Activar", "Desactivar", "Alternar" }, GUILayout.Width(90));

                        obj.action = (ArAction_EnableOrDisableObjects.ActionEnum)actionType;

                        if (GUILayout.Button(new GUIContent(removeIcon), EditorStyles.miniButton, GUILayout.Width(30)))
                        {
                            action.objectActions.Remove(obj);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5);

                Texture addIcon = EditorGUIUtility.IconContent("P4_AddedRemote").image;
                if (GUILayout.Button(new GUIContent(" Agregar Objeto", addIcon)))
                {
                    action.objectActions.Add(new ArAction_EnableOrDisableObjects.ObjectAction());
                }

            }
            EditorGUILayout.EndVertical();
        }

        private void DrawOpenUrlAction(ArAction_OpenUrl action)
        {
            action.url = EditorGUILayout.TextField("URL", action.url);
        }

        private void DrawPlaySoundAction(ArAction_PlaySound action)
        {
            int startWith = (int)action.startWith;

            startWith = EditorGUILayout.Popup("Evento Para Iniciar", startWith, new string[] { "Al hacer click en el objeto", "Cuando inicia la escena" });

            action.startWith = (ArActionBase.StartWithEnum)startWith;
            action.audioClip = (AudioClip)EditorGUILayout.ObjectField("Clip de Audio", action.audioClip, typeof(AudioClip), false);
            action.loop = EditorGUILayout.Toggle("Reproducir Constantemente", action.loop);

            GUILayout.Space(5);
            action.pauseOnDisable = EditorGUILayout.Toggle("Pausar mientras está desactivado", action.pauseOnDisable);
        }

        private void DrawRotateObjectAction(ArAction_RotateObject action)
        {
            int startWith = (int)action.startWith;

            startWith = EditorGUILayout.Popup("Evento Para Iniciar", startWith, new string[] { "Al hacer click en el objeto", "Cuando inicia la escena" });

            action.startWith = (ArActionBase.StartWithEnum)startWith;
            action.rotationAxis = (ArAction_RotateObject.RotationAxis)EditorGUILayout.EnumPopup("Eje de Rotación", action.rotationAxis);
            action.rotationSpeed = EditorGUILayout.FloatField("Velocidad de Rotación (grados/segundo)", action.rotationSpeed);


            int rotationMode = (int)action.rotationMode;

            rotationMode = EditorGUILayout.Popup("Modo de Rotación", rotationMode, new string[] { "Constantemente", "Por Tiempo Fijo", "Agregar Ángulo" });

            action.rotationMode = (ArAction_RotateObject.RotationMode)rotationMode;

            if (action.rotationMode == ArAction_RotateObject.RotationMode.Timed)
            {
                action.rotationDuration = EditorGUILayout.FloatField("Duración de Rotación (segundos)", action.rotationDuration);
            }
            else if (action.rotationMode == ArAction_RotateObject.RotationMode.AddAngle)
            {
                action.rotationAmount = EditorGUILayout.FloatField("Cantidad de Rotación (grados)", action.rotationAmount);
            }

            action.targetObject = (GameObject)EditorGUILayout.ObjectField("Objeto a Rotar", action.targetObject, typeof(GameObject), true);


            if (action.targetObject == null)
            {
                action.targetObject = script.gameObject;
            }
        }

        private void DrawMoveObjectAction(ArAction_MoveObject action)
        {
            int startWith = (int)action.startWith;

            startWith = EditorGUILayout.Popup("Evento Para Iniciar", startWith, new string[] { "Al hacer click en el objeto", "Cuando inicia la escena" });

            action.startWith = (ArActionBase.StartWithEnum)startWith;

            int movementMode = (int)action.movementMode;

            movementMode = EditorGUILayout.Popup("Modo de Movimiento", movementMode, new string[] { "Agregar Desplazamiento", "Establecer Posición", "Mover a Posición de Objeto" });

            action.movementMode = (ArAction_MoveObject.MovementMode)movementMode;

            if (action.movementMode == ArAction_MoveObject.MovementMode.AddOffset)
            {
                action.positionOffset = EditorGUILayout.Vector3Field("Desplazamiento", action.positionOffset);
            }
            else if (action.movementMode == ArAction_MoveObject.MovementMode.SetPosition)
            {
                action.targetPosition = EditorGUILayout.Vector3Field("Posición Objetivo", action.targetPosition);
            }
            else if (action.movementMode == ArAction_MoveObject.MovementMode.MoveToObject)
            {
                action.targetPositionObject = (GameObject)EditorGUILayout.ObjectField("Objeto de  Destino", action.targetPositionObject, typeof(GameObject), true);
            }

            action.movementDuration = EditorGUILayout.FloatField("Duración del Movimiento (segundos)", action.movementDuration);
            action.targetObject = (GameObject)EditorGUILayout.ObjectField("Objeto a Mover", action.targetObject, typeof(GameObject), true);

            if (action.targetObject == null)
            {
                action.targetObject = script.gameObject;
            }
        }


        private void DrawPlayAnimationAction(ArAction_PlayAnimation action)
        {
            int startWith = (int)action.startWith;

            startWith = EditorGUILayout.Popup("Evento Para Iniciar", startWith, new string[] { "Al hacer click en el objeto", "Cuando inicia la escena" });

            action.startWith = (ArActionBase.StartWithEnum)startWith;

            action.targetObject = (GameObject)EditorGUILayout.ObjectField("Objeto con Animaciones", action.targetObject, typeof(GameObject), true);

            // Si el targetObject es nulo, usar el objeto del script
            if (action.targetObject == null)
            {
                action.targetObject = script.gameObject;
            }

            if (action.targetObject != null)
            {
                // Lista para almacenar los nombres y referencias de las animaciones
                List<string> animationNames = new List<string>();
                List<AnimationClip> animationClips = new List<AnimationClip>();

                // Obtener los clips de animación del objeto
                AnimationClip[] clips = GetAnimationClipsFromObject(action.targetObject);

                if (clips.Length > 0)
                {
                    foreach (var clip in clips)
                    {
                        if (!clip.name.Contains("__preview__"))
                        {
                            animationNames.Add(clip.name);
                            animationClips.Add(clip);
                        }
                    }

                    // Encontrar el índice del clip actualmente seleccionado
                    int selectedIndex = animationClips.IndexOf(action.originalClip != null ? action.originalClip : action.animationClip);
                    if (selectedIndex == -1)
                    {
                        selectedIndex = 0;
                        action.originalClip = animationClips[0];
                        action.animationClip = animationClips[0];
                    }

                    EditorGUI.BeginChangeCheck();
                    selectedIndex = EditorGUILayout.Popup("Animación", selectedIndex, animationNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        AnimationClip selectedClip = animationClips[selectedIndex];
                        action.originalClip = selectedClip;

                        // Verificar si es necesario clonar y marcar como Legacy
                        action.animationClip = GetOrCreateLegacyClip(selectedClip);
                    }
                    else
                    {
                        // Si no se cambió la selección, comprobamos que tengamos un clip Legacy asignado
                        if (action.animationClip == null)
                        {
                            // Si está en null, lo asignamos por primera vez
                            action.animationClip = GetOrCreateLegacyClip(animationClips[selectedIndex]);
                        }
                        else
                        {
                            // si 'animationClip' no es Legacy, forzamos la conversión
                            if (!action.animationClip.legacy)
                            {
                                action.animationClip = GetOrCreateLegacyClip(action.animationClip);
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No se encontraron animaciones en el objeto seleccionado.", MessageType.Warning);

                    EditorGUI.BeginChangeCheck();
                    AnimationClip selectedClip = (AnimationClip)EditorGUILayout.ObjectField("Animación", action.originalClip != null ? action.originalClip : action.animationClip, typeof(AnimationClip), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        action.originalClip = selectedClip;
                        action.animationClip = GetOrCreateLegacyClip(selectedClip);
                    }
                    else if (action.animationClip == null && action.originalClip != null)
                    {
                        action.animationClip = GetOrCreateLegacyClip(action.originalClip);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Por favor, seleccione un objeto.", MessageType.Info);
                action.originalClip = (AnimationClip)EditorGUILayout.ObjectField("Animación", action.originalClip != null ? action.originalClip : action.animationClip, typeof(AnimationClip), false);

                // Asignar animationClip basado en originalClip
                if (action.originalClip != null)
                {
                    action.animationClip = GetOrCreateLegacyClip(action.originalClip);
                }
            }

            int animationMode = (int)action.animationMode;

            animationMode = EditorGUILayout.Popup("Modo de Animación", animationMode, new string[] { "Reproducir Una Vez", "Reproducir Constantemente", "Reproducir y Retornar (Ping Pong)" });

            action.animationMode = (ArAction_PlayAnimation.AnimationModeEnum)animationMode;
        }

        // Necesitamos agregar un nuevo campo en la clase ArAction_PlayAnimation para realizar un seguimiento del clip original seleccionado por el usuario
        // public AnimationClip originalClip;

        private AnimationClip GetOrCreateLegacyClip(AnimationClip clip)
        {
            if (clip == null)
                return null;

            // Verificar si ya hemos creado y almacenado un clip Legacy para este clip original
            AnimationClip existingLegacyClip = GetExistingLegacyClip(clip);
            if (existingLegacyClip != null)
            {
                return existingLegacyClip;
            }

            // Si el clip no es de solo lectura o ya es Legacy, no necesitamos clonarlo
            if (!IsReadOnly(clip) || clip.legacy)
            {
                return clip;
            }

            // Clonar y marcar como Legacy
            AnimationClip clonedClip = CloneAndMarkLegacy(clip);
            return clonedClip != null ? clonedClip : clip;
        }

        private AnimationClip GetExistingLegacyClip(AnimationClip originalClip)
        {
            // Buscar si ya existe un clip Legacy asociado
            string path = AssetDatabase.GetAssetPath(originalClip);
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                directory = "Assets";
            }

            // Sanitizar el nombre del clip para crear un nombre de archivo válido
            string sanitizedClipName = RemoveIllegalFileNameCharacters(originalClip.name + "_Legacy");

            string possiblePath = Path.Combine(directory, sanitizedClipName + ".anim");
            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(possiblePath);
            if (existingClip != null)
            {
                return existingClip;
            }

            // Si no se encuentra, buscar en todo el proyecto (opcional)
            string[] guids = AssetDatabase.FindAssets(sanitizedClipName);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (clip != null && clip.name == sanitizedClipName)
                {
                    return clip;
                }
            }

            return null;
        }

        private bool IsReadOnly(AnimationClip clip)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(path))
                return false;

            ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            return modelImporter != null; // Si es importado por ModelImporter, es ReadOnly
        }

        // Modificar CloneAndMarkLegacy para sanitizar el nombre del clip y manejar caracteres ilegales
        private AnimationClip CloneAndMarkLegacy(AnimationClip originalClip)
        {
            if (originalClip == null)
                return null;

            // Crear una copia del AnimationClip
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(originalClip, newClip);

            newClip.name = originalClip.name + "_Legacy";
            newClip.legacy = true; // Marcar como Legacy

            // Guardar el nuevo clip como un asset
            string originalPath = AssetDatabase.GetAssetPath(originalClip);
            string directory = Path.GetDirectoryName(originalPath);
            if (string.IsNullOrEmpty(directory))
            {
                directory = "Assets";
            }

            // Sanitizar el nombre del clip para crear un nombre de archivo válido
            string sanitizedClipName = RemoveIllegalFileNameCharacters(newClip.name);

            // Generar un nombre de ruta único para evitar sobreescritura
            string newPath = Path.Combine(directory, sanitizedClipName + ".anim");
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            try
            {
                AssetDatabase.CreateAsset(newClip, newPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al crear el asset del AnimationClip: " + e.Message);
                return null;
            }

            return newClip;
        }

        // Método para eliminar caracteres ilegales de un nombre de archivo
        private string RemoveIllegalFileNameCharacters(string filename)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                filename = filename.Replace(c.ToString(), "_");
            }
            return filename;
        }

        private AnimationClip[] GetAnimationClipsFromObject(GameObject obj)
        {
            List<AnimationClip> clips = new List<AnimationClip>();

            // Intentar obtener los clips desde el Animation
            Animation animation = obj.GetComponent<Animation>();
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (state.clip != null)
                    {
                        clips.Add(state.clip);
                    }
                }
            }

            // Si no se encontraron clips, intentar obtenerlos desde el modelo
            if (clips.Count == 0)
            {
                // Obtener la ruta del asset del modelo
                string assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(obj) ?? obj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    // Cargar todos los assets en esa ruta
                    UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (UnityEngine.Object asset in assets)
                    {
                        if (asset is AnimationClip clip)
                        {
                            if (!clip.name.Contains("__preview__")) // Excluir clips de vista previa
                            {
                                clips.Add(clip);
                            }
                        }
                    }
                }
            }

            return clips.ToArray();
        }

        private void DrawMakeCallAction(ArAction_MakeCall action)
        {
            action.phoneNumber = EditorGUILayout.TextField("Número de Teléfono", action.phoneNumber);
        }

        private void DrawSendTextMessageAction(ArAction_SendTextMessage action)
        {
            action.phoneNumber = EditorGUILayout.TextField("Número de Teléfono", action.phoneNumber);
            action.message = EditorGUILayout.TextField("Mensaje", action.message);
        }

        private void DrawOpenWhatsAppAction(ArAction_OpenWhatsApp action)
        {
            action.phoneNumber = EditorGUILayout.TextField("Número de Teléfono", action.phoneNumber);
            action.message = EditorGUILayout.TextField("Mensaje", action.message);
        }

        private void DrawSendEmailAction(ArAction_SendEmail action)
        {
            action.emailAddress = EditorGUILayout.TextField("Dirección de Correo", action.emailAddress);
            action.subject = EditorGUILayout.TextField("Asunto", action.subject);
            action.body = EditorGUILayout.TextField("Cuerpo del Mensaje", action.body);
        }

        void CheckErrorsForClick(GameObject obj)
        {
            // Verifica si el objeto tiene un componente RectTransform (es un elemento de UI)
            // Verifica también si es  TextMeshPro
            if (obj.GetComponent<RectTransform>() != null && obj.GetComponent<TMPro.TextMeshPro>() == null)
            {
                // Es un objeto de UI
                // Verifica si es cliqueable

                // Para que los elementos de UI sean cliqueables, deben tener un componente gráfico (Image, Text, etc.) con Raycast Target habilitado
                Graphic uiGraphic = obj.GetComponent<Graphic>();
                if (uiGraphic == null)
                {
                    ArActionsEditorTools.WarningWithBigFont("Este objeto de UI puede no ser cliqueable. Asegúrate de que tenga un componente de UI (Image, Text, etc.) con Raycast Target habilitado.");
                }
                else if (!uiGraphic.raycastTarget)
                {
                    ArActionsEditorTools.WarningWithBigFont("El componente gráfico tiene Raycast Target deshabilitado. Habilítalo para que el objeto sea cliqueable.");
                }
            }
            else
            {
                // Es un objeto normal (no UI)
                // Verifica si tiene un Collider o Collider2D
                Collider collider = obj.GetComponent<Collider>();
                Collider2D collider2D = obj.GetComponent<Collider2D>();

                if (collider == null && collider2D == null)
                {
                    ArActionsEditorTools.WarningWithBigFont("Este objeto no tiene un Collider. Debes agregar un Collider para definir el área del clic.");

                    if (GUILayout.Button("Agregar Collider", ArActionsEditorTools.BigButtonStyle))
                    {
                        obj.AddComponent<BoxCollider>();
                    }
                }
            }
        }
    }
}