using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _Toolkit.ResourceManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Toolkit.Config
{
    public static class ConfigManager
    {
        private static Dictionary<Type, IConfig> cache;

        public static async UniTask Load(string[] assetsPath, bool checkExist = true)
        {
            if (cache != null && checkExist)
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            
            Type[] allType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes()).Where(x =>
                {
                    if (typeof(IConfig).IsAssignableFrom(x) && !x.IsInterface)
                        return !x.IsAbstract;
                    return false;
                }).Select(x => x).ToArray();

            cache = new Dictionary<Type, IConfig>();
            
            UniTask<TextAsset>[] loadTasks = new UniTask<TextAsset>[assetsPath.Length];
            
            Type[] types = new Type[assetsPath.Length];

            for (int i = 0; i < assetsPath.Length; i++)
            {
                Type type = FindType(assetsPath[i], allType);

                if (type == null) continue;
                
                types[i] = type;

                loadTasks[i] = AssetLoader.GetAsset<TextAsset>(assetsPath[i]);
            }
            
            TextAsset[] assets = await UniTask.WhenAll(loadTasks);

            for (int i = 0; i < assetsPath.Length; i++)
            {
                Type type = types[i];
                
                object asset = JsonUtility.FromJson(assets[i].text, type);

                IConfig config = asset as IConfig;

                config.OnMappingValue();

                cache[type] = config;
            }

            sw.Stop();

            Debug.Log($"Load success '{assetsPath.Length}' config files, in {sw.ElapsedMilliseconds} ms");
        }

        private static Type FindType(string typeName, Type[] sources)
        {
            foreach (var type in sources)
            {
                if (string.Equals(typeName, type.Name)) return type;
            }

            return null;
        }

        public static T Get<T>() where T : class, IConfig
        {
            if (cache.TryGetValue(typeof(T), out IConfig config))
            {
                return config as T;
            }

            throw new TypeLoadException($"Unload config by type " + typeof(T));
        }
    }
}