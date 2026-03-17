using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KIT.Utils
{
    public abstract class KitEntryScene : MonoBehaviour
    {
        public static KitEntryScene Instance { get; private set; }

        [SerializeField] protected bool enableDebug = true;

        private AsyncOperation asyncOperation;
        private bool isLoading;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Debug.unityLogger.logEnabled = enableDebug;
            DontDestroyOnLoad(gameObject);
            Instance = this;
            OnAwake();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            string develop_mode = "DEVELOP_MODE";
            if (!enableDebug && DefineSymbolUtils.Has(develop_mode))
            {
                DefineSymbolUtils.Remove(develop_mode);
            }
            else if (enableDebug && !DefineSymbolUtils.Has(develop_mode))
            {
                DefineSymbolUtils.Add(develop_mode);
            }
#endif
        }

        private void Start()
        {
            CheckNewGame();
            InitService();
            OnStart();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }

        private void CheckNewGame()
        {
            string keyNewGame = "app_new_game";
            if (!PlayerPrefs.HasKey(keyNewGame))
            {
                PlayerPrefs.SetInt(keyNewGame, 1);
                OnNewGame();
            }
        }

        protected virtual void OnNewGame()
        {
        }

        protected virtual void InitService()
        {
        }

        protected virtual void ChangeSceneAsync(string sceneName)
        {
            if (isLoading) return;

            isLoading = true;
            asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
        }

        protected virtual void StopChangeScene()
        {
            if (!isLoading || asyncOperation == null)
                return;

            asyncOperation.allowSceneActivation = true;
            asyncOperation.completed += _ =>
            {
                asyncOperation = null;
                isLoading = false;
            };
        }

        public virtual void CloseLoadingScene()
        {
        }
    }
}