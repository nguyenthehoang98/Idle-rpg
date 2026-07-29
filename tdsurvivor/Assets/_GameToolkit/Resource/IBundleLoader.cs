using Cysharp.Threading.Tasks;

namespace _GameToolkit.Resource
{
    internal interface IBundleLoader
    {
        UniTask<bool> SyncAllBundles(int version);
       
        UniTask<T> GetAsset<T>(string assetName) where T : UnityEngine.Object;
      
        UniTask<T> GetAssetCached<T>(string assetName) where T : UnityEngine.Object;
       
        void UnCache(string assetName);
    }
}