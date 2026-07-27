using _GameToolkit.GameConfig;
using _GameToolkit.Resource;
using _GameToolkit.Startup;
using _TDS.GameConfig;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace _TDS.Startup
{
    public sealed class GameBootScene : BootScene
    {
        [SerializeField] private GameObject loadingScreenMasking;

        private bool isLoadingScene = false;
        private float elapsedTime = 1;
        
        protected async override void OnStart()
        {
            base.OnStart();
            
            AssetManager.SetAssetLocal();
            
            LoadSceneAsync("GamePlayScene");

            await ConfigManager.Load(new string[]
            {
                nameof(SpawnerConfig),
            });
        }

        private void Update()
        {
            if (isLoadingScene)
            {
                elapsedTime -= Time.deltaTime;

                if (elapsedTime <= 0)
                {
                    CloseLoadingScene();
                }
            }
        }

        protected override void OnStartLoadingScene()
        {
            base.OnStartLoadingScene();
            
            isLoadingScene = true;
            
            loadingScreenMasking.SetActive(true);
        }

        protected override void OnCloseLoadingScene()
        {
            base.OnCloseLoadingScene();
            
            loadingScreenMasking.SetActive(false);
            
            isLoadingScene = false;
        }
        
        
#if UNITY_EDITOR
        [MenuItem("Tools/Scenes/Boot Scene %Q")]
        private static void LoadBootScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            EditorSceneManager.OpenScene("Assets/_TDS assets/Scenes/BootScene.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Scenes/Home Scene %W")]
        private static void LoadHomeScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/_TDS assets/Scenes/HomeScene.unity");
        }

        [MenuItem("Tools/Scenes/Gameplay Scene %E")]
        private static void LoadGameplayScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.OpenScene("Assets/_TDS assets/Scenes/GamePlayScene.unity");
        }
#endif 
    }
}