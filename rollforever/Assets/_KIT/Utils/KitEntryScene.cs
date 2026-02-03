using UnityEngine;

namespace _KIT.Utils
{
    public abstract class KitEntryScene : MonoBehaviour
    {
        public static KitEntryScene Instance { get; private set; }

        [SerializeField] protected bool enableDebug = true;

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
        
        public virtual void ChangeScene(string sceneName)
        {
        }

        protected virtual void InitService()
        {
        }
    }
}