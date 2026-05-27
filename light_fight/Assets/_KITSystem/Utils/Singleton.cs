using UnityEngine;

namespace _KITSystem.Utils
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);

                        if (instance == null)
                        {
                            GameObject obj = new GameObject(typeof(T).Name);
                            instance = obj.AddComponent<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;

                // 🔥 Quan trọng: giữ qua scene
                DontDestroyOnLoad(gameObject);

                OnSingletonInit();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnSingletonInit()
        {
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                _isQuitting = true;
            }
        }
    }
}