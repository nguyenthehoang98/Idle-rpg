using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using _GameToolkit.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _GameToolkit.GameConfig
{
    public static class ConfigManager
    {
        private static Dictionary<Type, IGameConfig> cache;

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
                    if (typeof(IGameConfig).IsAssignableFrom(x) && !x.IsInterface)
                        return !x.IsAbstract;
                    return false;
                }).Select(x => x).ToArray();

            cache = new Dictionary<Type, IGameConfig>();
            
            UniTask<TextAsset>[] loadTasks = new UniTask<TextAsset>[assetsPath.Length];
            
            Type[] types = new Type[assetsPath.Length];

            for (int i = 0; i < assetsPath.Length; i++)
            {
                Type type = FindType(assetsPath[i], allType);

                if (type == null) continue;
                
                types[i] = type;

                loadTasks[i] = AssetManager.GetAsset<TextAsset>(assetsPath[i]);
            }
            
            TextAsset[] assets = await UniTask.WhenAll(loadTasks);

            for (int i = 0; i < assetsPath.Length; i++)
            {
                Type type = types[i];
                
                object asset = JsonUtility.FromJson(assets[i].text, type);

                IGameConfig config = asset as IGameConfig;

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