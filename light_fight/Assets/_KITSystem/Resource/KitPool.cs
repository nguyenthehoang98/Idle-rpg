using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KITSystem.Resource
{
    /// <summary>
    /// cần option object xóa ở trong scene.
    /// </summary>
    public partial class KitPool : MonoBehaviour
    {
        private static readonly Dictionary<string, Pool> dictionary = new Dictionary<string, Pool>();

        private static KitPool instance;

        public static KitPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("KitPool").AddComponent<KitPool>();
                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            List<string> keys = dictionary.Keys.ToList();
            foreach (var key in keys)
            {
                Pool pool = dictionary[key];
                if (pool.IsDestroyIfChangeScene)
                {
                    pool.Dispose();
                    // phải lưu cả object ko ở trong pool
                    dictionary.Remove(key);
                }
            }
        }

        #region Pool

        public static void RegisterPool(GameObject ins, bool isDestroyIfChangeScene)
        {
            if (ins == null)
            {
#if UNITY_EDITOR
                Debug.LogError("register pool ins is null");
#endif
                return;
            }

            string code = ins.name;

            if (dictionary.ContainsKey(code)) return;
            
            dictionary.Add(code, new Pool(Instance.transform, ins, isDestroyIfChangeScene));
        }

        public static void UnRegisterPool(GameObject ins)
        {
            string code = ins.name;
            
            if (dictionary.TryGetValue(code, out Pool pool))
            {
                pool.Dispose();
                
                dictionary.Remove(code);
            }
        }

        #endregion

        #region Instantiate

        public new static T Instantiate<T>(T ins) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject);
            return o.GetComponent<T>();
        }

        public new static T Instantiate<T>(T ins, Transform parent) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject);
            o.transform.SetParent(parent);
            return o.GetComponent<T>();
        }

        public static T Instantiate<T>(T ins, bool active) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject, active);
            return o.GetComponent<T>();
        }

        public static GameObject Instantiate(GameObject ins, bool active)
        {
            string code = ins.name;
            if (dictionary.TryGetValue(code, out Pool pool))
            {
                GameObject o = pool.Reuse();
                o.SetActive(active);
                return o;
            }

            GameObject obj = UnityEngine.Object.Instantiate(ins);
            obj.SetActive(active);
            obj.name = ins.name;
            return obj;
        }

        public static GameObject Instantiate(GameObject ins)
        {
            string code = ins.name;
            if (dictionary.TryGetValue(code, out Pool pool))
            {
                GameObject o = pool.Reuse();
                o.SetActive(true);
                return o;
            }

            GameObject obj = UnityEngine.Object.Instantiate(ins);
            obj.name = ins.name;
            return obj;
        }

        public static T Instantiate<T>(GameObject ins) where T : Component
        {
            GameObject obj = Instantiate(ins);
            if (obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Error instantiate {typeof(T)} from Object: " + ins);
#endif
                return null;
            }

            T component = obj.GetComponent<T>();
#if UNITY_EDITOR
            if (component == null) Debug.LogError($"Error parse to {typeof(T)} from Object: {obj.name}");
#endif
            return obj.GetComponent<T>();
        }

        public static async Task<T> Instantiate<T>(string path) where T : Component
        {
            GameObject obj = await KitLoaded.LoadAsync<GameObject>(path);
            if (obj == null)
                return null;
            return Instantiate<T>(obj);
        }

        #endregion

        public static void Destroy(GameObject ins)
        {
            if (ins == null) return;
            
            string code = ins.name;
            
            if (dictionary.TryGetValue(code, out Pool pool))
            {
                pool.Release(ins);
            }
            else
            {
                UnityEngine.Object.Destroy(ins);
            }
        }
    }
    
#if UNITY_EDITOR
    public partial class KitPool
    {
        public static IReadOnlyDictionary<string, Pool> EditorPools => new ReadOnlyDictionary<string, Pool>(dictionary);
    }
#endif
}