namespace Expiria3DSpace
{
    using Expiria3DSpace.UGSService.Controller;
    //using Unity.Netcode;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public class Expiria3DManager : Editor
    {
        static Expiria3DManager()
        {
            //If the user is not logged in, reset all the session info
            if (!User.IsLoggedIn)
            {
                User.Logout();
            }

            //This is do it with this method because the Update method is called only when Unity is completelly opened.
            //If the objects are searched directly here (In the initialization of the script), then will be return null objects because the scene is not completelly loaded.
            EditorApplication.update -= OnUnityInitializationComplete;
            EditorApplication.update += OnUnityInitializationComplete;

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }
        static void OnUndoRedoPerformed()
        {
            //repaint all the editor windows. This solves a headache for inspector and all the other windows that need to be updated when an undo/redo is performed.
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                window.Repaint();
            }
        }

        static void OnUnityInitializationComplete()
        {
            if (BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            //Here unity is completelly initialised
            // This is necessary for AR
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);

            if (ARManager.Instance != null)
            {
                if (ARManager.Instance.arProjectType == ARManager.ARProjectType.Image)
                {
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                }
                else
                {
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                }
            }

            if (!EditorPrefs.GetBool("ExpiriaExecuteOnFirstOpenOnly-" + Application.productName + "-" + Application.companyName + "-" + Application.dataPath, false))
            {
                EditorPrefs.SetBool("ExpiriaExecuteOnFirstOpenOnly-" + Application.productName + "-" + Application.companyName + "-" + Application.dataPath, true);
            }
            else
            {
                //If the user is not logged in, reset all the session info
                if (!User.IsLoggedIn)
                {
                    User.Logout();
                }
            }

            //-------------------------------------
            EditorApplication.update -= OnUnityInitializationComplete; //Unsubscribe again to avoid to continue calling in the update.
        }
    }
}
