using UnityEditor;
using UnityEngine;

namespace _Game.Home.Editor
{
    [CustomEditor(typeof(HomeScene))]
    public sealed class HomeSceneEditor : UnityEditor.Editor
    {
        private SerializedProperty homeCanvas;
        private SerializedProperty playButton;
        private SerializedProperty tabContainer;
        private SerializedProperty tabs;
        private SerializedProperty initialTabIndex;
        private SerializedProperty activeScale;
        private SerializedProperty tabClickCooldown;

        private void OnEnable()
        {
            homeCanvas = serializedObject.FindProperty("homeCanvas");
            playButton = serializedObject.FindProperty("playButton");
            tabContainer = serializedObject.FindProperty("tabContainer");
            tabs = serializedObject.FindProperty("tabs");
            initialTabIndex = serializedObject.FindProperty("initialTabIndex");
            activeScale = serializedObject.FindProperty("activeScale");
            tabClickCooldown = serializedObject.FindProperty("tabClickCooldown");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Home References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(homeCanvas, new GUIContent("Canvas"));
            EditorGUILayout.PropertyField(playButton, new GUIContent("Play Button"));
            EditorGUILayout.PropertyField(tabContainer, new GUIContent("Tab Container"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Tabs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Add one entry per page. Keep the list order identical to " +
                "Tab Container children, then drag each ButtonHomeMenu, Content, and BaseTab here. " +
                "ButtonHomeMenu finds and controls its own focus animations.",
                MessageType.Info);
            EditorGUILayout.PropertyField(tabs, new GUIContent("Tab Entries"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialTabIndex, new GUIContent("Initial Tab Index"));
            EditorGUILayout.PropertyField(activeScale);
            EditorGUILayout.PropertyField(tabClickCooldown, new GUIContent("Click Cooldown"));

            serializedObject.ApplyModifiedProperties();
        }

    }
}
