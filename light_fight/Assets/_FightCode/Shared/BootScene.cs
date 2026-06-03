using _KITSystem.Data;
using _KITSystem.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _FightCode.Shared
{
    public class BootScene : EntryScene
    {
        private float elapsed = 1;
        private bool isLoadingScene = false;
        
        [SerializeField] private GameObject loadingScene;
        
        protected override void OnNewGame()
        {
            // todo: khởi tạo userdata
        }

        protected override void InitService()
        {
            // Tạo các service mạng
        }

        protected override async void OnStart()
        {
            await ConfigManager.Load(new[]
            {
                "SkillConfig",
                "LevelConfig",
                "MonsterConfig",
                "WeaponConfig",
            });

            ChangeSceneAsync("Home Scene");
            
            isLoadingScene = true;
        }

        private void Update()
        {
            if (isLoadingScene)
            {
                elapsed -= Time.deltaTime;
                if (elapsed <= 0)
                {
                    StopChangeScene();
                    isLoadingScene = false;
                }
            }
        }

        public override void HideLoadingScene()
        {
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(false);
            }
        }

        public override void ShowLoadingScene()
        {
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(true);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Scenes/Boot Scene %Q")]
        private static void LoadBootScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            EditorSceneManager.OpenScene("Assets/_FightSource/Scenes/Boot Scene.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Scenes/Home Scene %W")]
        private static void LoadHomeScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/_FightSource/Scenes/Home Scene.unity");
        }

        [MenuItem("Tools/Scenes/Gameplay Scene %E")]
        private static void LoadGameplayScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/_FightSource/Scenes/Battle Scene.unity");
        }
#endif 
    }
}
