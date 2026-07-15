using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using K4os.Compression.LZ4;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _KITSystem.Config
{
    public static class ConfigManager
    {
        private static Dictionary<Type, IGameConfig> cache;

        public static async UniTask Load(string[] scriptObjectsPath, bool checkExist = true)
        {
            if (cache != null && checkExist)
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            Type[] allType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes()).Where(x =>
                {
                    if (typeof(IGameConfig).IsAssignableFrom(x) && !x.IsInterface)
                        return !x.IsAbstract;
                    return false;
                }).Select(x => x).ToArray();

            cache = new Dictionary<Type, IGameConfig>();

            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                Type type = FindType(scriptObjectsPath[i], allType);

                if (type == null) continue;

                byte[] bytes = (await AssetBundleManager.GetAsset<TextAsset>(scriptObjectsPath[i])).bytes;

                byte[] unpick = LZ4Pickler.Unpickle(bytes); 

                string text = Encoding.UTF8.GetString(unpick);

                object asset = JsonUtility.FromJson(text, type);

                IGameConfig config = asset as IGameConfig;

                config.OnMappingValue();

                cache[type] = config;
            }

            sw.Stop();

            Debug.Log($"Load success '{scriptObjectsPath.Length}' config files, in {sw.ElapsedMilliseconds} ms");
        }

        private static Type FindType(string typeName, Type[] sources)
        {
            foreach (var type in sources)
            {
                if (string.Equals(typeName, type.Name)) return type;
            }

            return null;
        }

        public static T Get<T>() where T : class, IGameConfig
        {
            if (cache.TryGetValue(typeof(T), out IGameConfig config))
            {
                return config as T;
            }

            throw new TypeLoadException($"Unload config by type " + typeof(T));
        }
    }
}