using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _GameToolkit.GameConfig
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
            
            UniTask<TextAsset>[] loadTasks = new UniTask<TextAsset>[scriptObjectsPath.Length];
            
            Type[] types = new Type[scriptObjectsPath.Length];

            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                Type type = FindType(scriptObjectsPath[i], allType);

                if (type == null) continue;
                
                types[i] = type;
                
                loadTasks[i] = Resources.LoadAsync<TextAsset>(scriptObjectsPath[i])
                    .ToUniTask()
                    .ContinueWith(x => x as TextAsset);
            }
            
            TextAsset[] assets = await UniTask.WhenAll(loadTasks);

            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                Type type = types[i];
                
                object asset = JsonUtility.FromJson(assets[i].text, type);

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