
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using _KIT.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KIT.Pool
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
                    KitPool kitPoolObject = GameObject.FindObjectOfType<KitPool>();
                    if (kitPoolObject == null)
                    {
                        kitPoolObject = new GameObject("KitPool").AddComponent<KitPool>();
                    }

                    instance = kitPoolObject;
                    DontDestroyOnLoad(kitPoolObject);
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
            foreach (var kvp in dictionary.Where(kvp => kvp.Value.IsDestroyIfChangeScene).ToList())
            {
                kvp.Value.Dispose();
                dictionary.Remove(kvp.Key);
            }
        }

        #region Pool

        public static void RegisterPool(GameObject ins, bool isDestroyIfChangeScene)
        {
            if (ins == null) return;

            string code = ins.name;
            if (!dictionary.ContainsKey(code))
            {
                dictionary.Add(code, new Pool(Instance.transform, ins, isDestroyIfChangeScene));
            }
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

        public static GameObject Instantiate(GameObject ins)
        {
            string code = ins.name;
            if (dictionary.TryGetValue(code, out Pool pool))
            {
                return pool.Reuse();
            }

            GameObject obj = UnityEngine.Object.Instantiate(ins);
            obj.name = ins.name;
            return obj;
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
