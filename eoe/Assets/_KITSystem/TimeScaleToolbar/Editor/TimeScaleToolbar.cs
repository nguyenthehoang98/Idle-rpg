using System;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace _KIT.Editor
{
    [InitializeOnLoad]
    public static class ToolbarTimeScale
    {
        private static float timeScale = 1f;
        
        static ToolbarTimeScale()
        {
            timeScale = Time.timeScale;
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            float field = EditorGUILayout.FloatField("", timeScale, GUILayout.Width(30), GUILayout.Height(19));
            if (Math.Abs(field - timeScale) > 0)
            {
                timeScale = field;
                Time.timeScale = field;
            }

            if (Math.Abs(timeScale - Time.timeScale) > 0)
            {
                timeScale = Time.timeScale;
            }
        }
    }
}