#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using _TDS.UI;

namespace _TDS.Editor
{
    public static class UiPreviewMenu
    {
        private const string PreviewScenePath = "Assets/Scenes/UiPreviewScene.unity";

        [MenuItem("Tools/TDS/UI/Create Preview Scene")]
        public static void CreatePreviewScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("UiPreview");
            root.AddComponent<UiPreviewScreen>();
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            Selection.activeGameObject = root;
            Debug.Log($"UI preview scene created at {PreviewScenePath}. Press Play to preview the base flow.");
        }

        [MenuItem("Tools/TDS/UI/Open Preview Scene")]
        public static void OpenPreviewScene()
        {
            if (!System.IO.File.Exists(PreviewScenePath))
            {
                CreatePreviewScene();
                return;
            }

            EditorSceneManager.OpenScene(PreviewScenePath);
        }
    }
}
#endif
