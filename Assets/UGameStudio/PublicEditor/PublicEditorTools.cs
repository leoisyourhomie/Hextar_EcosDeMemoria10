using UnityEditor;

namespace UGSSpace
{
    public static class PublicEditorTools
    {
        public static void PauseEditor()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
        }

        public static void PlayBeep()
        {
#if UNITY_EDITOR
            EditorApplication.Beep();
#endif
        }
    }
}