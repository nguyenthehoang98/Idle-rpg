using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Checker.Editor
{
    public abstract class BaseEditorWindowChart : EditorWindow
    {
        // tab
        private int tab;
        
        // Chart
        private bool shouldDrawChart = false;
        private bool showHelpbox = false;
        private string errorMessage = "";
        protected ChartData chartData = new ChartData();

        private void DrawChart()
        {
            if (shouldDrawChart)
            {
                Rect chartRect = GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                );

                chartData.layoutRect = chartRect;

                using (var view = new ChartView(chartData))
                {
                    view.DrawChart();
                }
            }
            else if (showHelpbox)
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }

        protected void EnableChart()
        {
            shouldDrawChart = true;
            showHelpbox = false;
        }

        protected void LogErrorChart(string message)
        {
            showHelpbox = true;
            errorMessage = message;
        }

        private void OnGUI()
        {
            DrawToolbar();

            int length = ToolbarNames().Length;
            for (int i = 0; i < length; i++)
            {
                if(tab == i) OnDrawTab(i);
            }
            
            DrawChart();
        }

        private void DrawToolbar()
        {
            int prev = tab;
            tab = GUILayout.Toolbar(tab, ToolbarNames());
            if (prev != tab) OnChangeToolbar();
            EditorGUILayout.Space(10);
        }

        protected abstract string[] ToolbarNames();

        protected abstract void OnDrawTab(int tabIndex);

        protected virtual void OnChangeToolbar()
        {
            shouldDrawChart = showHelpbox = false;
        }
    }
}