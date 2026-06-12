namespace UGSSpace
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class SwipeDirectionsSelectorWindow : PopupWindowContent
    {
        private List<SwipeDirectionInfo> currentDirs;

        private Action<List<SwipeDirectionInfo>> onDirectionListChanged;

        public SwipeDirectionsSelectorWindow(List<SwipeDirectionInfo> directionList, Action<List<SwipeDirectionInfo>> onDirectionListChanged)
        {
            currentDirs = directionList;
            this.onDirectionListChanged = onDirectionListChanged;
        }

        public override void OnClose()
        {
            onDirectionListChanged?.Invoke(currentDirs);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 400);
        }

        public override void OnGUI(Rect rect)
        {
            Rect r = EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();

            foreach (SwipeDirectionInfo direction in currentDirs)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 85;
                direction.IsSelected = EditorGUILayout.Toggle("[" + (int)direction.Direction + "] " + direction.Direction.ToString(), direction.IsSelected);
                EditorGUIUtility.labelWidth = 30;
                direction.MinAngle = EditorGUILayout.FloatField("Min", direction.MinAngle);
                direction.MaxAngle = EditorGUILayout.FloatField("Max", direction.MaxAngle);
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                onDirectionListChanged?.Invoke(currentDirs);
            }

            if (GUILayout.Button("Reset Angles"))
            {
                currentDirs = new()
                {
                    new SwipeDirectionInfo(SwipeDirection.Up, 67.5f, 112.5f),
                    new SwipeDirectionInfo(SwipeDirection.UpRight, 22.5f, 67.5f),
                    new SwipeDirectionInfo(SwipeDirection.Right, 337.5f, 22.5f),
                    new SwipeDirectionInfo(SwipeDirection.DownRight, 292.5f, 337.5f),
                    new SwipeDirectionInfo(SwipeDirection.Down, 247.5f, 292.5f),
                    new SwipeDirectionInfo(SwipeDirection.DownLeft, 202.5f, 247.5f),
                    new SwipeDirectionInfo(SwipeDirection.Left, 157.5f, 202.5f),
                    new SwipeDirectionInfo(SwipeDirection.UpLeft, 112.5f, 157.5f)
                };

                onDirectionListChanged?.Invoke(currentDirs);
            }
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndVertical();

            float circleRadius = 50;
            Vector2 circleCenter = new Vector2(this.editorWindow.position.width / 2f, r.position.y + r.height + circleRadius + 20);
            Rect circleRect = new Rect(circleCenter.x - circleRadius * 1.5f, circleCenter.y - circleRadius, circleRadius * 3f, circleRadius * 3f);

            foreach (SwipeDirectionInfo direction in currentDirs)
            {
                if (!direction.IsSelected)
                {
                    continue;
                }

                float startAngle = direction.MinAngle;
                float endAngle = direction.MaxAngle;
                Color color = GetColorForSwipeDirection(direction.Direction);

                // We have a bug when drawing angles where the end angle is smaller than the start angle
                // So we need to draw the angle in two parts
                if (endAngle < startAngle)
                {
                    startAngle = 0;

                    Texture2D fillTexture = CreateFillTexture(circleRadius, startAngle, endAngle, color);
                    GUI.DrawTexture(circleRect, fillTexture);

                    startAngle = direction.MinAngle;
                    endAngle += 360;

                    fillTexture = CreateFillTexture(circleRadius, startAngle, endAngle, color);
                    GUI.DrawTexture(circleRect, fillTexture);
                }
                else
                {
                    Texture2D fillTexture = CreateFillTexture(circleRadius, startAngle, endAngle, color);
                    GUI.DrawTexture(circleRect, fillTexture);
                }
            }
        }

        private Texture2D CreateFillTexture(float radius, float startAngle, float endAngle, Color color)
        {
            int textureSize = 100;
            Texture2D texture = new Texture2D(textureSize, textureSize);

            float startAngleRadians = startAngle * Mathf.Deg2Rad;
            float endAngleRadians = endAngle * Mathf.Deg2Rad;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 positionFromCenter = new Vector2(x, y) - new Vector2(radius, radius);
                    float angle = Mathf.Atan2(positionFromCenter.y, positionFromCenter.x);
                    if (angle < 0f)
                    {
                        angle += Mathf.PI * 2f;
                    }

                    if (positionFromCenter.magnitude <= radius && angle >= startAngleRadians && angle <= endAngleRadians)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private Color GetColorForSwipeDirection(SwipeDirection swipeDirection)
        {
            // Provide color mappings for each swipe direction
            switch (swipeDirection)
            {
                case SwipeDirection.Right:
                    return Color.red;
                case SwipeDirection.UpRight:
                    return Color.green;
                case SwipeDirection.Up:
                    return Color.blue;
                case SwipeDirection.UpLeft:
                    return Color.yellow;
                case SwipeDirection.Left:
                    return Color.magenta;
                case SwipeDirection.DownLeft:
                    return Color.cyan;
                case SwipeDirection.Down:
                    return Color.gray;
                case SwipeDirection.DownRight:
                    return Color.white;
                default:
                    return Color.white;
            }
        }
    }
}