using System;
using _KITSystem.ExcelConfig;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _FightCode.Config
{
    public abstract class BaseConfig : KitBaseConfig
    {
#if UNITY_EDITOR
        protected void LoadObject<T>(string path, Action<T> callback)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                Addressables.LoadAssetAsync<T>(path).Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                        Debug.LogError($"[{GetType().Name}] Not found {typeof(T).Name} with '{path}'");
                    else 
                        callback?.Invoke(handle.Result);
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetType().Name}] Not found {typeof(T).Name} with '{path}'");
            }
        } 
        
        protected void ValidateObject<T>(string path)
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