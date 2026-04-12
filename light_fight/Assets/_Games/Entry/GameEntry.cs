using _KIT.Config;
using _KIT.Utils;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace _Games.Entry
{
    public class GameEntry : KitEntryScene
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
            await KitConfigManager.Load(new[]
            {
                "MonsterConfig",
                "SkillConfig",
                "LevelConfig",
                "WeaponConfig",
            });

            ChangeSceneAsync("HomeScene");
            
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
            EditorSceneManager.OpenScene("Assets/Scenes/BootScene.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Scenes/Home Scene %W")]
        private static void LoadHomeScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/Scenes/HomeScene.unity");
        }

        [MenuItem("Tools/Scenes/Gameplay Scene %E")]
        private static void LoadGameplayScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/Scenes/GamePlayScene.unity");
        }

        [MenuItem("Tools/Scenes/Automatic Scene %4")]
        private static void LoadAutomaticScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/Scenes/AutomaticScene.unity");
        }
#endif 
    }
}