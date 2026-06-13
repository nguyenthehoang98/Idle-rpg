using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _KITSystem.Resource
{
    internal class CloudBundleLoader : IBundleLoader
    {
        private const string ConfigFileName = "BundleConfig.txt";
        
        private const string ConfigPath = "{0}/" + ConfigFileName;

        private string databaseUrl = "https://pub-9a80a47b61434822ac3497289cec7280.r2.dev/StandaloneWindows64";

        private readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

        public IReadOnlyCollection<string> LoadedBundleNames => bundles.Keys;

        public void SetDatabaseUrl(string url) => databaseUrl = url;

        public UniTask<T> GetAsset<T>(string assetName) where T : UnityEngine.Object
        {
            foreach (var pair in bundles)
            {
                T asset = pair.Value.LoadAsset<T>(assetName);
                if (asset != null) return UniTask.FromResult(asset);
            }

            return new UniTask<T>(null);
        }

        public UniTask<T> GetThenAsset<T>(string assetName) where T : Object
        {
            return GetAsset<T>(assetName);
        }

        public void UnCache(string assetName)
        {
        }

        public async UniTask<bool> SyncAllBundles(int version)
        {
            int saved = PlayerPrefs.GetInt("BundleVersion", 0);

            if (saved == version)
            {
                if (await LoadBundles())
                    return true;
            }

            return await DownloadAll(version);
        }

        private async UniTask<bool> DownloadAll(int version)
        {
            string context = await DownloadConfig(version);
            if (context == null) return false;

            await DownloadBundles(version, context);

            PlayerPrefs.SetInt("BundleVersion", version);
            PlayerPrefs.Save();

            CleanOrphanedFiles(context);
            return true;
        }

        private async UniTask<string> DownloadConfig(int version)
        {
            string path = GetFileLocalPath(ConfigFileName);
            string url = $"{databaseUrl}/{string.Format(ConfigPath, version)}";

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                req.downloadHandler = new DownloadHandlerFile(path);
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AssetBundleManager] Download config fail: {url}, {req.result}");
                    return null;
                }
            }

            return File.ReadAllText(path);
        }

        private async UniTask DownloadBundles(int version, string context)
        {
            string[] bundleNames = context.Split('\n');

            foreach (var name in bundleNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (bundles.ContainsKey(name)) continue;

                await DownloadBundle(version, name);
            }
        }

        private async UniTask DownloadBundle(int version, string bundleName)
        {
            if (bundles.TryGetValue(bundleName, out AssetBundle old))
            {
                old.Unload(true);
                bundles.Remove(bundleName);
                await Resources.UnloadUnusedAssets();
            }

            string path = GetFileLocalPath($"{bundleName}.bundle");
            string url = $"{databaseUrl}/{version}/{bundleName}.bundle";

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                req.downloadHandler = new DownloadHandlerFile(path);
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AssetBundleManager] Download bundle fail '{bundleName}': {url}, {req.result}");
                    return;
                }
            }

            AssetBundle bundle = await UnityAsyncExtensions.ToUniTask(AssetBundle.LoadFromFileAsync(path));
            if (bundle == null)
            {
                Debug.LogError($"[AssetBundleManager] Load bundle fail '{bundleName}'");
                return;
            }

            bundles[bundleName] = bundle;
            Debug.Log($"[AssetBundleManager] Downloaded '{bundleName}'");
        }

        private async UniTask<bool> LoadBundles()
        {
            var (ok, names) = LoadConfigFromFile();
            if (!ok) return false;

            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (bundles.ContainsKey(name)) continue;

                string path = GetFileLocalPath($"{name}.bundle");
                if (!File.Exists(path))
                {
                    Debug.LogError($"[AssetBundleManager] Bundle not found locally: {name}");
                    return false;
                }

                if (bundles.TryGetValue(name, out AssetBundle old))
                {
                    old.Unload(true);
                    bundles.Remove(name);
                }

                AssetBundle bundle = await UnityAsyncExtensions.ToUniTask(AssetBundle.LoadFromFileAsync(path));
                if (bundle == null)
                {
                    Debug.LogError($"[AssetBundleManager] Load bundle failed: {name}");
                    return false;
                }

                bundles[name] = bundle;
                Debug.Log($"[AssetBundleManager] Loaded '{name}'");
            }

            return true;
        }

        private (bool ok, string[] names) LoadConfigFromFile()
        {
            string path = GetFileLocalPath(ConfigFileName);
            if (!File.Exists(path)) return (false, new string[0]);

            string[] lines = File.ReadAllLines(path);
            return (lines.Length > 0, lines);
        }

        private void CleanOrphanedFiles(string context)
        {
            string folder = GetDirectoryLocalPath();
            var keep = new HashSet<string> { ConfigFileName };

            foreach (var name in context.Split('\n'))
            {
                if (!string.IsNullOrEmpty(name))
                    keep.Add(name + ".bundle");
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                string name = Path.GetFileName(file);
                if (!keep.Contains(name))
                {
                    File.Delete(file);
                    Debug.Log("[AssetBundleManager] Deleted orphaned: " + name);
                }
            }
        }

        private string GetFileLocalPath(string assetName)
        {
            string folder = Application.persistentDataPath + "/Bundles";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return $"{folder}/{assetName}";
        }

        private string GetDirectoryLocalPath()
        {
            string folder = Application.persistentDataPath + "/Bundles";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }
    }
}