

using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Startup;
using _TDS.GameConfig;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
namespace _TDS.Boot
{
    public class GameEntry : BootScene
    {
        private float elapsedTime = 1;
        
        private bool isLoadingScene = false;
        
        [SerializeField] private GameObject loadingScene;

        protected override async void OnStart()
        {
            base.OnStart();
            
            AssetLoader.SetAssetLocal();
            
            LoadSceneAsync("GameplayScene");
            
            isLoadingScene = true;

            await ConfigManager.Load(new string[]
            {
                nameof(MonsterConfig), 
                nameof(SpawnConfig), 
                nameof(WeaponConfig), 
                nameof(PlayerConfig),  
            });
        }

        private void Update()
        {
            if (isLoadingScene)
            {
                elapsedTime -= Time.deltaTime;
              
                if (elapsedTime <= 0) CloseLoadingScene();
            }
        }
        
        protected override void OnStartLoadingScene()
        {
            base.OnStartLoadingScene();
            
            isLoadingScene = true;
            
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(true);
            }
        }
        
        protected override void OnCloseLoadingScene()
        {
            base.OnCloseLoadingScene();
            
            if (loadingScene != null && loadingScene.activeInHierarchy)
            {
                loadingScene.gameObject.SetActive(false);
            }
            
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