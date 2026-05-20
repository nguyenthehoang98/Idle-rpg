using System;
using System.Collections.Generic;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _KITSystem.ExcelConfig
{
    public static class KitConfigManager
    {
        private static Dictionary<Type, KitBaseConfig> cache;

        public static async UniTask Load(string[] scriptObjectsPath, bool checkExist = true)
        {
            if (cache != null && checkExist)
            {
                return;
            }
            
            cache = new Dictionary<Type, KitBaseConfig>();
            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                int index = i;
                ScriptableObject asset = await KitLoaded.LoadAsync<ScriptableObject>(scriptObjectsPath[i]);
                if (asset is KitBaseConfig config)
                {
                    config.OnMapValue();
                    cache.Add(config.GetType(), config);
                }
                else
                {
                    Debug.LogError($"Error parse to {typeof(KitBaseConfig)} from: " + scriptObjectsPath[index]);
                }
            }
        }

        public static T Get<T>() where T : KitBaseConfig
        {
            if (cache.TryGetValue(typeof(T), out KitBaseConfig config))
            {
                return config as T;
            }

            throw new TypeLoadException($"Unload config by type " + typeof(T));
        }
    }
}