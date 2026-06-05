using System;
using System.Collections.Generic;
using System.Linq;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

                string text = (await KitLoaded.LoadAsync<TextAsset>(scriptObjectsPath[i])).text;

                object asset = JsonUtility.FromJson(text, type);

                cache[type] = asset as IGameConfig;
            }

            Debug.Log($"Load success '{scriptObjectsPath.Length}' config files.");
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