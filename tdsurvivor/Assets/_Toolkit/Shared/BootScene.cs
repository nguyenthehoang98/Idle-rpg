using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Toolkit.Shared
{
    public class BootScene : MonoBehaviour
    {
        public static BootScene Instance { get; private set; }

        [SerializeField] private bool logEnabled;
        [SerializeField] private bool gizmosEnabled;

        protected bool IsLoadingScene { get; private set; }

        public bool GizmosEnabled
        {
            get { return gizmosEnabled; }
        }

        protected AsyncOperation AsyncOperation { get; private set; }

        private void Awake()
        {
            Instance = this;

            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Debug.unityLogger.logEnabled = logEnabled;
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

            AsyncOperation = SceneManager.LoadSceneAsync(sceneName);

            if (AsyncOperation != null)
            {
                IsLoadingScene = true;

                AsyncOperation.allowSceneActivation = false;

                OnStartLoadingScene();
            }
        }

        public void CloseLoadingScene()
        {
            if (IsLoadingScene && AsyncOperation != null)
            {
                AsyncOperation.allowSceneActivation = true;

                AsyncOperation.completed += operation =>
                {
                    IsLoadingScene = false;

                    AsyncOperation = null;

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