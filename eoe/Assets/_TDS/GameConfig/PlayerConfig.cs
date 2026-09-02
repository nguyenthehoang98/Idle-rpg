using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable]
    public class PlayerConfig : IConfig
    {
        [SerializeField] private List<HeroData> heros = new List<HeroData>();
        [SerializeField] private List<WingData> wings = new List<WingData>();
        [SerializeField] private List<PlayerExpData> exp = new List<PlayerExpData>();

        private Dictionary<int, PlayerExpData> cachedExp;
        private Dictionary<int, HeroData> cachedHero;
        private Dictionary<int, WingData> cachedWing;

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

            cachedWing = new Dictionary<int, WingData>();
            foreach (var wing in wings)
            {
                cachedWing.Add(wing.id, wing);
            }
        }

        public void OnImported()
        {
            foreach (var heroData in heros)
            {
                bool found = false;
                foreach (var wingData in wings)
                {
                    if (wingData.id == heroData.wingId)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) Debug.LogError($"Not found wing data '{heroData.wingId}' at hero {heroData.id}");
            }
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetHero(int heroId, out HeroData data)
        {
            return cachedHero.TryGetValue(heroId, out data);
        }

        public bool TryGetWing(int wingId, out WingData data)
        {
            return cachedWing.TryGetValue(wingId, out data);
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
        public int wingId;
    }

    [Serializable]
    public struct WingData
    {
        public int id;
        public string wingName;
    }

    [Serializable]
    public struct PlayerExpData
    {
        public int level;
        public int exp;
    }
}
