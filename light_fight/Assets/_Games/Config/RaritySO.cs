using System;
using System.Collections.Generic;
using _Games.Misc.Model;
using UnityEngine;

namespace _Games.Config
{
    [CreateAssetMenu(menuName = "Rarity SO")]
    public class RaritySO : ScriptableObject
    {
        [SerializeField] private Data[] rarities;
        
        private Dictionary<Rarity, Data> dict;

        public Sprite GetBorderRarity(Rarity rarity)
        {
            if (dict == null)
            {
                dict = new Dictionary<Rarity, Data>();
                foreach (var icon in rarities)
                {
                    dict.Add(icon.rarity, icon);
                }
            }
            
            if (dict.TryGetValue(rarity, out Data data))
                return data.border;
            throw new NullReferenceException($"GetBorderRarity error '{name}', Rarity '{rarity}' not found.");
        }

        [Serializable]
        class Data
        {
            public Rarity rarity;
            public Sprite border;
        }
    }
}