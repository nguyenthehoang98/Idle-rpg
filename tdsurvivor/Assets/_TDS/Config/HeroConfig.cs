using System;
using System.Collections.Generic;
using ExcelExtension;
using _Toolkit.Config;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable, ExcelAsset(
         ExcelPath = "Assets/Excels/HeroConfig.xlsx",
         ConfigPath = "Assets/_TDS assets/Config/HeroConfig.json")]
    public class HeroConfig : IConfig
    {
        [SerializeField] private List<HeroConfigData> heroes = new List<HeroConfigData>();

        Dictionary<int, HeroConfigData> cachedHeroes;

        public void OnMappingValue()
        {
            cachedHeroes = new Dictionary<int, HeroConfigData>();

            foreach (var h in heroes)
            {
                cachedHeroes[h.id] = h;
            }
        }

        public void OnImported()
        {
        }

        public void OnCompleteImported()
        {
        }

        public bool TryGetHero(int id, out HeroConfigData data)
        {
            return cachedHeroes.TryGetValue(id, out data);
        }

        public IReadOnlyDictionary<int, HeroConfigData> GetAll()
        {
            return cachedHeroes;
        }
    }
}
