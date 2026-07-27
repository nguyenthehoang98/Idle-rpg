using UnityEngine;
using UnityEngine.SceneManagement;

namespace _GameToolkit.Startup
{
    public class BootScene : MonoBehaviour
    {
        public static BootScene Instance { get; private set; }

        [SerializeField] private bool debugEnabled = false;
        
        protected bool IsLoadingScene { get; private set; }

        private AsyncOperation asyncOperation;

        protected AsyncOperation AsyncOperation
        {
            get => asyncOperation;
        }

        private void Awake()
        {
            Instance = this;

            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Debug.unityLogger.logEnabled = debugEnabled;
            DontDestroyOnLoad(gameObject);
            
            OnAwake();
        }

        private void Start()
        {
            const string key = "application.new.game";
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, 1);
                NewGame();
            }
            
            InitializeServices();
            OnStart();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void InitializeServices()
        {
        }

        protected virtual void NewGame()
        {
        }

        public void LoadSceneAsync(string sceneName)
        {
            if (IsLoadingScene) return;

            asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            if(asyncOperation != null)
            {
                IsLoadingScene = true;
                
                asyncOperation.allowSceneActivation = false;
                
                OnStartLoadingScene();
            }
        }

        public void CloseLoadingScene()
        {
            if (IsLoadingScene && asyncOperation != null)
            {
                asyncOperation.allowSceneActivation = true;

                asyncOperation.completed += operation =>
                {
                    IsLoadingScene = false;
                    
                    asyncOperation = null;
                    
                    OnCloseLoadingScene();
                };
            }
        }

        protected virtual void OnCloseLoadingScene()
        {
        }

        protected virtual void OnStartLoadingScene()
        {
        }
    }
}
