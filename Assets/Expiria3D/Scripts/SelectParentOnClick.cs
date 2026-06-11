using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectParentOnClick : MonoBehaviour
{
    // Start is called before the first frame update
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (Selection.activeGameObject == this.gameObject)
        {
            UnityEditor.Selection.activeGameObject = transform.root.gameObject;
        }
#endif
    }
}
