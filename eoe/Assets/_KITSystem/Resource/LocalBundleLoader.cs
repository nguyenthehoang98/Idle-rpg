using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;
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
                Debug.Log($"[Loaded] Uncached asset: {assetName}");
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
            Stopwatch sw = Stopwatch.StartNew();
#endif

            try
            {
                handle = Addressables.LoadAssetAsync<T>(assetName);

                await handle.ToUniTask();

                T asset = handle.Result;

                if (asset == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[Loaded] Asset at path '{assetName}' is null. \n\n{stackTrace}");
#else
                    Debug.LogError($"[Loaded] Asset at path '{assetName}' is null.");
#endif

                    return null;
                }

#if UNITY_EDITOR
                sw.Stop();
#endif
                
                if (cached)
                {
                    Dictionary[assetName] = new CacheEntry(handle, asset);
#if UNITY_EDITOR
                    Debug.Log($"[Loaded] Cached asset: {assetName}, duration '{sw.ElapsedMilliseconds}'ms\n\n{stackTrace}");
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
#if UNITY_EDITOR
                sw.Stop();
                Debug.LogError($"[Loaded] Failed to load asset '{typeof(T)}' at path '{assetName}'\n\n{stackTrace}");
                Debug.LogError(e);
#else
                Debug.LogError($"[Loaded] Failed to load asset '{typeof(T)}' at path '{assetName}'");
                Debug.LogError(e);
#endif
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