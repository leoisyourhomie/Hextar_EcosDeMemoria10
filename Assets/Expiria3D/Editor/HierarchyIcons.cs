namespace Expiria3DSpace
{

    using UnityEditor;
    using UnityEngine;
    using System;
    [InitializeOnLoad]
    public class HierarchyIcons
    {
        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons;
        }

        static bool scale = false;
        private static void EvaluateIcons(int instanceId, Rect selectionRect)
        {
            bool isArScene = GameObject.FindFirstObjectByType<ARManager>() != null;

            if (EditorPrefs.GetBool("EnableHierarchyIcons", true))
            {
                GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

                if (go == null)
                    return;

                if (selectionRect.y == 16) //This ensure to start with black
                    scale = true;

                Texture2D t = new Texture2D(1, 1);
                Color color;

                if (!EditorGUIUtility.isProSkin)
                {
                    if (scale)
                    {
                        color = new Color(0, 0, 0, 0.02f);
                        scale = false;
                    }
                    else
                    {
                        color = new Color(0, 0, 0, 0.04f);
                        scale = true;
                    }
                }
                else
                {
                    if (scale)
                    {
                        color = new Color(100, 200, 100, 0.011f);
                        scale = false;
                    }
                    else
                    {
                        color = new Color(100, 200, 100, 0.022f);
                        scale = true;
                    }
                }

                bool isArTarget = go.GetComponent<IARTarget>() != null;
                bool isChildOfTracker = go.transform.root.GetComponent<IARTarget>() != null;

                t.SetPixel(1, 1, color);
                t.Apply();

                GUI.DrawTexture(new Rect(0, selectionRect.y, Screen.width, selectionRect.height), t, ScaleMode.ScaleAndCrop);

                if (!isArTarget)
                {
                    if (isArScene && !isChildOfTracker && go.GetComponent<Camera>() == null && !go.transform.root.name.Contains("Canvas"))
                    {
                        t.SetPixel(1, 1, Color.red);
                        t.Apply();

                        GUI.DrawTexture(new Rect(0, selectionRect.y, 3, selectionRect.height), t, ScaleMode.ScaleAndCrop);
                        DrawIcon("", selectionRect, EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D);

                        //Draw a text that says (Error)
                        DrawTxt("(Sin Seguimiento)", selectionRect, go.transform, 0);
                    }
                }
                else
                {
                    DrawIcon("ar", selectionRect);
                    t.SetPixel(1, 1, Color.yellow);
                    t.Apply();
                    GUI.DrawTexture(new Rect(0, selectionRect.y, 3, selectionRect.height), t, ScaleMode.ScaleAndCrop);
                }
                //selectionRect.x = selectionRect.width - 20;
                //selectionRect.width = selectionRect.x+20;
                //GUI.Label(selectionRect, EditorGUIUtility.ObjectContent(go.GetComponent<Transform>(), typeof(Transform)).image);
            }
        }

        static void DrawTxt(string txt, Rect rect, Transform t, int offset)
        {
            float labelWidthCalc = 0;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow * 0.8f;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleRight;

            labelWidthCalc = style.CalcSize(new GUIContent(txt)).x;


            GUI.Label(new Rect(rect.width, rect.y - 2, labelWidthCalc - 50, 20), txt, style);
        }

        private static void DrawIcon(string texName, Rect rect, Texture2D specificTexture = null)
        {
            Texture2D texture = specificTexture == null ? GetTex(texName) : specificTexture;

            Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);

            if (texture != null)
                GUI.DrawTexture(r, texture);
        }

        private static Texture2D GetTex(string name)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Expiria3D/EditorIcons/" + name + ".png");

            if (texture == null)
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Expiria3D/EditorIcons/ArActions/" + name + ".png");

            return texture;
        }
    }

}
