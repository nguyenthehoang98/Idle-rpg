using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace _KITSystem.Resource
{
    public class Pool : IDisposable
    {
        public bool IsDestroyIfChangeScene { get; }
#if UNITY_EDITOR
        public string Code { get; }
        public int Total => allObjects.Count;
        public int ActiveCount { get; private set;}
        public readonly List<GameObject> allObjects = new List<GameObject>();
#endif
        
        private readonly Transform parent;
        private GameObject instance;
        private readonly IObjectPool<GameObject> pool;

        public Pool(Transform dontDestroyParent, GameObject instance, bool isDestroyIfChangeScene)
        {
            if (isDestroyIfChangeScene)
            {
                parent = new GameObject().transform;
#if UNITY_EDITOR
                parent.name = "Group " + instance.name;
#endif
            }
            else
            {
                parent = dontDestroyParent;                
            }

            this.instance = instance;
#if UNITY_EDITOR
            Code = instance.name;
#endif
            IsDestroyIfChangeScene = isDestroyIfChangeScene;
            pool = new ObjectPool<GameObject>(OnCreate, OnReuse, OnRelease, OnDestroy);
        }

        public void Release(GameObject target)
        {
#if UNITY_EDITOR
            ActiveCount--;
#endif
            pool.Release(target);
        }

        public void Preload(int total)
        {
            for (int i = 0; i < total; i++)
            {
                GameObject go = Reuse();
                Release(go);
            }
        }

        public GameObject Reuse()
        {
#if UNITY_EDITOR
            ActiveCount++;
#endif
            GameObject go = pool.Get();
            return go;
        }

        private void OnDestroy(GameObject go)
        {
#if UNITY_EDITOR
            allObjects.Remove(go);
#endif
            Object.Destroy(go);
        }

        private void OnRelease(GameObject go)
        {
            go.transform.SetParent(parent);
            go.SetActive(false);
        }

        private void OnReuse(GameObject go)
        {
            go.transform.SetParent(null);
            go.SetActive(true);
        }

        private GameObject OnCreate()
        {
            GameObject go = Object.Instantiate(instance);
            go.name = instance.name;
#if UNITY_EDITOR
            allObjects.Add(go);
#endif
            return go;
        }

        public void Dispose()
        {
            pool.Clear();
            instance = null;
        }
    }
}