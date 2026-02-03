using System;
using System.Collections.Generic;
using _KIT.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _KIT.Config
{
    public interface IConfig
    {
        void OnMapValue();
    }

    public static class KitConfigManager
    {
        private static Dictionary<Type, IConfig> cache;

        public static async UniTask Load(string[] scriptObjectsPath, bool checkExist = true)
        {
            if (cache != null && checkExist)
            {
                return;
            }
            
            cache = new Dictionary<Type, IConfig>();
            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                int index = i;
                ScriptableObject asset = await KitLoaded.LoadAsync<ScriptableObject>(scriptObjectsPath[i]);
                if (asset is IConfig config)
                {
                    config.OnMapValue();
                    cache.Add(config.GetType(), config);
                }
                else
                {
                    Debug.LogError($"Error parse to {typeof(IConfig)} from: " + scriptObjectsPath[index]);
                }
            }
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