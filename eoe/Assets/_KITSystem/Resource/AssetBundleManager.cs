using Cysharp.Threading.Tasks;

namespace _KITSystem.Resource
{
    public static class AssetBundleManager
    {
        private static IBundleLoader loader;
        private static bool isInitialized = false;

        public static void SetLocationBundle(bool fromLocal)
        {
            if (isInitialized) return;

            isInitialized = true;

            if (fromLocal)
                loader = new LocalBundleLoader();
            else
                loader = new CloudBundleLoader();
        }

        public static UniTask<bool> SyncAllBundles(int version)
        {
            if (isInitialized) return loader.SyncAllBundles(version);
            else
                return UniTask.FromResult(false);
        }

        public static UniTask<T> GetAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (isInitialized) return loader.GetAsset<T>(assetName);
            else
                return UniTask.FromResult<T>(null);
        }

        public static UniTask<T> GetAssetCached<T>(string assetName) where T : UnityEngine.Object
        {
            if (isInitialized) return loader.GetAssetCached<T>(assetName);
            else
                return UniTask.FromResult<T>(null);
        }

        public static void UnCache(string assetName)
        {
            if (isInitialized) loader.UnCache(assetName);
        }
    }
}