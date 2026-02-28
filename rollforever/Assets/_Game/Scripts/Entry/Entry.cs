using _KIT.Config;
using _KIT.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace _Game.Scripts.Entry
{
    public class Entry : KitEntryScene
    {
#if UNITY_EDITOR
        [Range(0.2f, 20.0f)] public float speed = 1f;
#endif
        private float elapsed = 1;
        private bool isLoadingScene = false;
        
        [SerializeField] private GameObject container;
        
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
                "WeaponConfig",
                "BuffConfig",
            });

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

        public override void CloseLoadingScene()
        {
            if (container != null && container.activeInHierarchy)
            {
                container.gameObject.SetActive(false);
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

            EditorSceneManager.OpenScene("Assets/Scenes/GameplayScene.unity");
        }
#endif 
    }
}