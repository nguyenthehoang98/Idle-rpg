using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
namespace _Game.Entry
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
            AssetBundleManager.SetLocationBundle(true);
            
            await ConfigManager.Load(new string[] { "MonsterConfig", "LevelConfig", "WeaponConfig", "PlayerConfig" });

            AssetBundleManager.GetAssetCached<GameObject>(Const.TEXT_DAMAGE_NORMAL);
            AssetBundleManager.GetAssetCached<GameObject>(Const.TEXT_DAMAGE_CRITICAL);

            ColorSetting.Load();

            ChangeSceneAsync("GameplayScene");
            
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
#endif 
    }
}