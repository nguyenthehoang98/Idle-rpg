using Cysharp.Threading.Tasks;

namespace _KITSystem.Resource
{
    internal interface IBundleLoader
    {
        UniTask<bool> SyncAllBundles(int version);
        UniTask<T> GetAsset<T>(string assetName) where T : UnityEngine.Object;
        UniTask<T> GetThenAsset<T>(string assetName) where T : UnityEngine.Object;
        void UnCache(string assetName);
    }
}