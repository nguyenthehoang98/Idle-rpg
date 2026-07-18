using System;
using System.Collections.Generic;
using _KITSystem.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace _Game.Configs
{
    [Serializable]
    public class MonsterConfig : IGameConfig
    {
        [SerializeField, JsonProperty] private List<MonsterData> monsters = new List<MonsterData>();

        private Dictionary<int, MonsterData> cached;

        public void OnMappingValue()
        {
            cached = new Dictionary<int, MonsterData>();

            foreach (var data in monsters)
            {
                if (!cached.TryAdd(data.id, data)) Debug.LogError($"Duplicate monster '{data.id}'");
            }
        }

        public void OnPostImported()
        {
        }

        public void OnValidateLinkConfig()
        {
        }

        public bool TryGetMonsterData(int monsterId, out MonsterData monsterData)
        {
            return cached.TryGetValue(monsterId, out monsterData);
        }
    }

    [Serializable]
    public struct MonsterData
    {
        public int id;
        public string prefabName;
        public int health;
        public int attack;
        public int exp;
        public float speed;
        public float stopDistance;
        public string deathAudioClip;
        public float deathVolume;
        public string deathVfx;
    }
}
