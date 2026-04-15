using System;
using _KIT.Config;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Games.Config
{
    public abstract class BaseConfig : KitBaseConfig
    {
#if UNITY_EDITOR
        protected void Load<T>(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                Addressables.LoadAssetAsync<T>(path).Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                        Debug.LogError($"[{GetType().Name}] Not found {typeof(T).Name} with '{path}'");
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetType().Name}] Not found {typeof(T).Name} with '{path}'");
            }
        }
#endif
    }
}