using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace _GameToolkit.ResourceManagement
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
            }
        }

        private async UniTask<T> GetAsset<T>(string assetName, bool cached) where T : Object
        {
            if (cached && Dictionary.TryGetValue(assetName, out var entry))
            {
                return entry.Asset as T;
            }

            AsyncOperationHandle<T> handle;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(assetName);

                await handle.ToUniTask();

                T asset = handle.Result;

                if (asset == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[Loaded] Asset at path '{assetName}' is null. \n\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}");
#else
                    Debug.LogError($"[Loaded] Asset at path '{assetName}' is null.");
#endif

                    return null;
                }

                if (cached)
                {
                    Dictionary[assetName] = new CacheEntry(handle, asset);
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
                Debug.LogError($"[Loaded] Failed to load asset '{typeof(T)}' at path '{assetName}'\n\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}");
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