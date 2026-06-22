using System;
using System.Collections.Generic;
using _KITSystem.Config;
using UnityEngine;

namespace _Game.Configs
{
    [Serializable]
    public class PlayerConfig : IGameConfig
    {
        [SerializeField] private List<PlayerExpData> exp = new List<PlayerExpData>();

        private Dictionary<int, PlayerExpData> cachedExp;
        
        public void OnMappingValue()
        {
            cachedExp = new Dictionary<int, PlayerExpData>();
            foreach (var pair in exp)
            {
                cachedExp.Add(pair.level, pair);
            }
        }

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
        }

        public bool TryGetExp(int level, out PlayerExpData data)
        {
            return cachedExp.TryGetValue(level, out data);
        }
    }

    [Serializable]
    public struct PlayerExpData
    {
        public int level;
        public int exp;
    }
}