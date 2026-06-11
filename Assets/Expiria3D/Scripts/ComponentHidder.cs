
namespace Expiria3DSpace
{
    using System.Collections;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    public class ComponentHidder : MonoBehaviour
    {
#if UNITY_EDITOR
        public List<string> componentsToHide = new List<string>();
        public bool disabled = false;

        void Reset()
        {
            HideComponents();
        }

        void OnDrawGizmosSelected()
        {
            HideComponents();
        }

        void OnValidate()
        {
            HideComponents();
        }

        void HideComponents()
        {
            foreach (string componentName in componentsToHide)
            {
                Component component = GetComponent(componentName);
                if (component != null)
                {
                    if (EditorPrefs.GetBool("UGS_AdvancedMode", false) == true || disabled == true)
                    {
                        component.hideFlags = HideFlags.None;
                    }
                    else
                    {
                        component.hideFlags = HideFlags.HideInInspector;
                    }
                }
            }

            //hide this
            if (EditorPrefs.GetBool("UGS_AdvancedMode", false) == true || disabled == true)
            {
                hideFlags = HideFlags.None;
            }
            else
            {
                hideFlags = HideFlags.HideInInspector;
            }
        }
#endif
    }
}