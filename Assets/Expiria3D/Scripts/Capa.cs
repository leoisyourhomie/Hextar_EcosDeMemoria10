
namespace Expiria3DSpace
{
    using UnityEngine;
    using System.Collections.Generic;

    public class Capa : MonoBehaviour
    {
        public int capa = 0;
        Canvas canvas;

        void OnDrawGizmosSelected()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvas != null)
            {
                canvas.sortingOrder = capa;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(canvas);
#endif
        }
    }
}