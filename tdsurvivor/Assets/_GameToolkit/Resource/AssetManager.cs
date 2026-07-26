using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameToolkit.Resource
{
    public static class AssetManager
    {
        private static IBundleLoader loader;

        private static bool isInitialized = false;

        public static void SetAssetLocal()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                loader = new LocalBundleLoader();
            }
            else
            {
                Debug.LogError("AssetBundleManager already initialized");
            }
        }

        public static void SetAssetCloud(string databaseUri)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                loader = new CloudBundleLoader(databaseUri);
            }
            else
            {
                Debug.LogError("AssetBundleManager already initialized");
            }
        }

        public static UniTask<bool> SyncAllBundles(int version)
        {
            if (isInitialized)
            {
                return loader.SyncAllBundles(version);
            }

            throw new Exception("AssetBundleManager not initialized");
        }

        public static UniTask<T> GetAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (isInitialized)
            {
                return loader.GetAsset<T>(assetName);
            }

            throw new Exception("AssetBundleManager not initialized");
        }

        public static UniTask<T> GetAssetCached<T>(string assetName) where T : UnityEngine.Object
        {
            if (isInitialized)
            {
                return loader.GetAssetCached<T>(assetName);
            }

            throw new Exception("AssetBundleManager not initialized");
        }

        public static void UnCache(string assetName)
        {
            if (isInitialized) loader.UnCache(assetName);
        }
    }
}