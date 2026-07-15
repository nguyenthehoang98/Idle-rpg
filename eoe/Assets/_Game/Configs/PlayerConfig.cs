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

        public void OnPostImported()
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

        public void OnValidateLinkConfig()
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
        [SerializeField] private int i;

        public int id
        {
            get => i;
            set => i = value;
        }

        [SerializeField] private string pn;

        public string prefabName
        {
            get => pn;
            set => pn = value;
        }

        [SerializeField] private int wi;

        public int wingId
        {
            get => wi;
            set => wi = value;
        }
    }

    [Serializable]
    public struct WingData
    {
        [SerializeField] private int i;

        public int id
        {
            get => i;
            set => i = value;
        }

        [SerializeField] private string wn;

        public string wingName
        {
            get => wn;
            set => wn = value;
        }
    }

    [Serializable]
    public struct PlayerExpData
    {
        [SerializeField] private int l;

        public int level
        {
            get => l;
            set => l = value;
        }

        [SerializeField] private int e;

        public int exp
        {
            get => e;
            set => e = value;
        }
    }
}