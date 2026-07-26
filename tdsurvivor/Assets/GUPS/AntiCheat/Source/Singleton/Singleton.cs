// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Singleton
{
    /// <summary>
    /// Thread-safe MonoBehaviour singleton base. Either persists across scene loads or lives only in the current scene.
    /// </summary>
    /// <typeparam name="T">The concrete singleton type.</typeparam>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public abstract class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        /// <summary>
        /// Backing field for <see cref="Instance"/>.
        /// </summary>
        private static T instance;

        /// <summary>
        /// Lock guarding singleton creation.
        /// </summary>
        private static object lockHandle = new object();

        /// <summary>
        /// Gets a value indicating whether the singleton survives scene loads
        /// (<see cref="Object.DontDestroyOnLoad(Object)"/>).
        /// </summary>
        public abstract bool IsPersistent { get; }

        /// <summary>
        /// Prevents recreating the singleton while the application is quitting.
        /// </summary>
        private static bool isQuitting = false;

        /// <summary>
        /// Gets the singleton, finding an existing instance in the scene or creating a fresh GameObject as needed.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (lockHandle)
                {
                    // Drop the cached reference if the GameObject was destroyed.
                    if (instance != null && instance.gameObject == null)
                    {
                        instance = null;
                    }

                    if (instance == null)
                    {
#if UNITY_2023_1_OR_NEWER
                        var instances = FindObjectsByType(typeof(T), FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                        var instances = FindObjectsOfType(typeof(T));
#endif

                        if (instances.Length > 0)
                        {
                            instance = instances[0] as T;
                        }

                        if (instance == null)
                        {
                            Create<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a singleton instance currently exists.
        /// </summary>
        public static bool Exists
        {
            get
            {
                return instance != null;
            }
        }

        /// <summary>
        /// Enforces uniqueness: destroys duplicate GameObjects, marks the canonical instance as
        /// <see cref="Object.DontDestroyOnLoad(Object)"/> when persistent and renames it.
        /// </summary>
        protected virtual void Awake()
        {
            if (Exists)
            {
                if (this != instance && this.gameObject != null)
                {
                    DestroyImmediate(this.gameObject);
                }
            }
            else
            {
                instance = this as T;

                if (instance.IsPersistent)
                {
                    instance.gameObject.name = "(PersistentSingleton) " + typeof(T).Name.ToString();

                    DontDestroyOnLoad(instance.gameObject);
                }
                else
                {
                    instance.gameObject.name = "(Singleton) " + typeof(T).Name.ToString();
                }
            }
        }

        /// <summary>
        /// Creates a fresh GameObject hosting the singleton component.
        /// </summary>
        /// <typeparam name="T1">Concrete subtype to instantiate.</typeparam>
        private static void Create<T1>() where T1 : T
        {
            if (Exists)
            {
                return;
            }

            // Skip creation when not in play mode or during shutdown.
            if (!Application.isPlaying || isQuitting)
            {
                return;
            }

            GameObject var_Singleton = new GameObject();

            instance = var_Singleton.AddComponent<T1>();

            if (instance.IsPersistent)
            {
                instance.gameObject.name = "(PersistentSingleton) " + typeof(T).Name.ToString();

                DontDestroyOnLoad(instance.gameObject);
            }
            else
            {
                instance.gameObject.name = "(Singleton) " + typeof(T).Name.ToString();
            }
        }

        /// <summary>
        /// Sets the quit flag so the singleton is not re-created during application shutdown.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}
