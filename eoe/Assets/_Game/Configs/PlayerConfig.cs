using System;
using System.Collections.Generic;
using _KITSystem.Config;
using UnityEngine;

namespace _Game.Configs
{
    [Serializable]
    public class PlayerConfig : IGameConfig
    {
        [SerializeField] private List<HeroData> heros = new List<HeroData>();
        [SerializeField] private List<PlayerExpData> exp = new List<PlayerExpData>();

        private Dictionary<int, PlayerExpData> cachedExp;
        private Dictionary<int, HeroData> cachedHero;
        
        public void OnMappingValue()
        {
            cachedExp = new Dictionary<int, PlayerExpData>();
            foreach (var pair in exp)
            {
                cachedExp.Add(pair.level, pair);
            }
            
            cachedHero = new Dictionary<int, HeroData>();
            foreach (var hero in heros)
            {
                cachedHero.Add(hero.id, hero);
            }
        }

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
        }

        public bool TryGetHero(int heroId, out HeroData data)
        {
            return cachedHero.TryGetValue(heroId, out data);
        }

        public bool TryGetExp(int level, out PlayerExpData data)
        {
            return cachedExp.TryGetValue(level, out data);
        }
    }

    [Serializable]
    public struct HeroData
    {
        public int id;
        public string prefabName;
        public string wingName;
    }

    [Serializable]
    public struct PlayerExpData
    {
        public int level;
        public int exp;
    }
}