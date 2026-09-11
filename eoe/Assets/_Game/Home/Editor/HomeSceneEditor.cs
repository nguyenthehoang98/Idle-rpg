using UnityEditor;
using UnityEngine;

namespace _Game.Home.Editor
{
    [CustomEditor(typeof(HomeScene))]
    public sealed class HomeSceneEditor : UnityEditor.Editor
    {
        private SerializedProperty homeCanvas;
        private SerializedProperty playButton;
        private SerializedProperty tabScrollRect;
        private SerializedProperty tabScrollSnap;
        private SerializedProperty tabs;
        private SerializedProperty initialTabIndex;
        private SerializedProperty activeScale;
        private SerializedProperty tabTransitionDuration;

        private void OnEnable()
        {
            homeCanvas = serializedObject.FindProperty("homeCanvas");
            playButton = serializedObject.FindProperty("playButton");
            tabScrollRect = serializedObject.FindProperty("tabScrollRect");
            tabScrollSnap = serializedObject.FindProperty("tabScrollSnap");
            tabs = serializedObject.FindProperty("tabs");
            initialTabIndex = serializedObject.FindProperty("initialTabIndex");
            activeScale = serializedObject.FindProperty("activeScale");
            tabTransitionDuration = serializedObject.FindProperty("tabTransitionDuration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Home References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(homeCanvas, new GUIContent("Canvas"));
            EditorGUILayout.PropertyField(playButton, new GUIContent("Play Button"));
            EditorGUILayout.PropertyField(tabScrollRect, new GUIContent("Tab Scroll Rect"));
            EditorGUILayout.PropertyField(tabScrollSnap, new GUIContent("Tab Scroll Snap"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Tabs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Add one entry per page. Keep the list order identical to " +
                "Tab Scroll Rect / Viewport / Content children, then drag each Button and Content here.",
                MessageType.Info);
            EditorGUILayout.PropertyField(tabs, new GUIContent("Tab Entries"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialTabIndex, new GUIContent("Initial Tab Index"));
            EditorGUILayout.PropertyField(activeScale);
            EditorGUILayout.PropertyField(tabTransitionDuration);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
