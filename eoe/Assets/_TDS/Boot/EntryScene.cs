

using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Startup;
using _TDS.GameConfig;
using _TDS.Gameplay;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
namespace _TDS.Boot
{
    public class EntryScene : BootScene
    {
        [SerializeField] private GameObject loadingScene;

        protected override async void OnStart()
        {
            base.OnStart();
            
            AssetLoader.SetAssetLocal();
            GameProgress.Load();
            OfflineReward offlineReward = GameProgress.LastOfflineReward;
            if (offlineReward.HasReward)
            {
                Debug.Log($"[Progress] Offline reward: +{offlineReward.Experience} EXP +{offlineReward.Gold} GOLD ({offlineReward.Minutes} min)");
            }

            await ConfigManager.Load(new[]
            {
                nameof(MonsterConfig),
                nameof(SpawnConfig),
                nameof(SkillConfig),
                nameof(ExpConfig),
                nameof(HeroConfig),
                nameof(UpgradeConfig),
            });

            LoadSceneAsync("HomeScene");
            CloseLoadingScene();
        }
        
        protected override void OnStartLoadingScene()
        {
            base.OnStartLoadingScene();
            
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(true);
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) GameProgress.Save();
        }

        private void OnApplicationQuit()
        {
            GameProgress.Save();
        }

        protected override void OnCloseLoadingScene()
        {
            base.OnCloseLoadingScene();
            
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(false);
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