using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _KITSystem.Resource
{
    /// <summary>
    /// A safe and optimized Addressables loader with optional caching.
    /// Handles release logic automatically and prevents memory leaks.
    /// </summary>
    public static class KitLoaded
    {
        private static readonly Dictionary<string, CacheEntry> dictionary = new Dictionary<string, CacheEntry>();

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

        /// <summary>
        /// Load an Addressable asset asynchronously.
        /// If cached = true, future calls return the same loaded instance.
        /// </summary>
        public static async UniTask<T> LoadAsync<T>(string path, bool cached = false) where T : class
        {
            // Return from dictionary
            if (dictionary.TryGetValue(path, out var entry))
            {
                return entry.Asset as T;
            }

            AsyncOperationHandle<T> handle;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(path);
                
                await handle.ToUniTask();
                
                T asset = handle.Result;
                
                if (asset == null)
                {
                    Debug.LogError($"[KitLoaded] Asset at path '{path}' is null.");
                    return null;
                }

                if (cached)
                {
                    dictionary[path] = new CacheEntry(handle, asset);
#if UNITY_EDITOR
                    Debug.Log($"[KitLoaded] Cached asset: {path}");
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
                Debug.LogError($"[KitLoaded] Failed to load asset '{typeof(T)}' at path '{path}'");
                Debug.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// Remove one cached asset and release its memory.
        /// </summary>
        public static void UnCache(string path)
        {
            if (dictionary.TryGetValue(path, out var entry))
            {
                dictionary.Remove(path);
                Addressables.Release(entry.Handle);
#if UNITY_EDITOR
                Debug.Log($"[KitLoaded] Uncached asset: {path}");
#endif
            }
        }

        /// <summary>
        /// Check whether an asset is cached.
        /// </summary>
        public static bool IsCached(string path) => dictionary.ContainsKey(path);
    }
}
