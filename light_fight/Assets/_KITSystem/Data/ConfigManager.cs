using System;
using System.Collections.Generic;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _KITSystem.Data
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
            
            cache = new Dictionary<Type, IGameConfig>();
            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                int index = i;
                TextAsset asset = await KitLoaded.LoadAsync<TextAsset>(scriptObjectsPath[i]);
                
                // type
                // data
            }
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

    public interface IGameConfig
    {
        void OnMappingValue();
        void OnPostImported();
    }
}