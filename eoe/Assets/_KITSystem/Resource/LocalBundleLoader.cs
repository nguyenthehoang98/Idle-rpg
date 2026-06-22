using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace _KITSystem.Resource
{
    internal class LocalBundleLoader : IBundleLoader
    {
        public UniTask<bool> SyncAllBundles(int version)
        {
            return UniTask.FromResult(true);
        }

        public UniTask<T> GetAsset<T>(string assetName) where T : Object
        {
            return GetAsset<T>(assetName, false);
        }

        public UniTask<T> GetAssetCached<T>(string assetName) where T : Object
        {
            return GetAsset<T>(assetName, true);
        }

        public void UnCache(string assetName)
        {
            if (Dictionary.TryGetValue(assetName, out var entry))
            {
                Dictionary.Remove(assetName);

                Addressables.Release(entry.Handle);

#if UNITY_EDITOR
                Debug.Log($"[KitLoaded] Uncached asset: {assetName}");
#endif
            }
        }

        private async UniTask<T> GetAsset<T>(string assetName, bool cached) where T : Object
        {
            if (cached && Dictionary.TryGetValue(assetName, out var entry))
            {
                return entry.Asset as T;
            }

            AsyncOperationHandle<T> handle;

#if UNITY_EDITOR
            string stackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
#endif

            try
            {
                handle = Addressables.LoadAssetAsync<T>(assetName);

                await handle.ToUniTask();

                T asset = handle.Result;

                if (asset == null)
                {
                    Debug.LogError($"[KitLoaded] Asset at path '{assetName}' is null. \n\n{stackTrace}");

                    return null;
                }

                if (cached)
                {
                    Dictionary[assetName] = new CacheEntry(handle, asset);
#if UNITY_EDITOR
                    Debug.Log($"[KitLoaded] Cached asset: {assetName}\n\n{stackTrace}");
#endif
                }
                else
                {
                    // Immediately release if not caching
                    handle.Completed += _ => Addressables.Release(handle);
                }

                return asset;
            }
            catch (Exception e)
            {
                Debug.LogError($"[KitLoaded] Failed to load asset '{typeof(T)}' at path '{assetName}'\n\n{stackTrace}");
                Debug.LogError(e);
                return null;
            }
        }

        private static readonly Dictionary<string, CacheEntry> Dictionary = new Dictionary<string, CacheEntry>();

        private class CacheEntry
        {
            public AsyncOperationHandle Handle;
            public object Asset;

            public CacheEntry(AsyncOperationHandle handle, object asset)
            {
                Handle = handle;
                Asset = asset;
            }
        }
    }
}