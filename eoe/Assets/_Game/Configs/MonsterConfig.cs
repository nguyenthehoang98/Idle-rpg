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
        [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();
        [JsonProperty] private List<MonsterScaleData> monster_scale = new List<MonsterScaleData>();
        [JsonProperty] private List<BaseMonsterData> monster_base = new List<BaseMonsterData>();

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
            for (var i = 0; i < monster_scale.Count; i++)
            {
                var data = monster_scale[i];
                monster_scale[i] = data;
            }

            monsters = new List<MonsterData>();
            for (int i = 0; i < monster_scale.Count; i++)
            {
                MonsterScaleData data = monster_scale[i];

                bool found = false;

                foreach (var b in monster_base)
                {
                    if (b.prefabName == data.baseId)
                    {
                        monsters.Add(new MonsterData
                        {
                            id = data.id,
                            skin = data.skin,
                            prefabName = b.prefabName,
                            speed = b.speed * data.scaleSpeed,
                            radius = b.radius * data.scaleRadius,
                            scale = data.scaleRadius,
                            stopDistance = data.stopDistance,
                            deathAudioClip = b.deathAudioClip,
                            deathVolume = b.deathVolume,
                            exp = data.expScale * b.exp,
                            attack = data.attackScale * b.attack,
                            health = data.healthScale * b.health,
                            deathVfx = b.deathVfx
                        });
                        found = true;
                        break;
                    }
                }

                if (!found) Debug.LogError($"Not found monster data with id '{data.id}', base '{data.baseId}'");
            }
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
        public string skin;
        public float speed;
        public float scale;
        public float radius;
        public float stopDistance;
        public string deathAudioClip;
        public float deathVolume;
        public string deathVfx;
        public float attack;
        public float health;
        public float exp;
    }

    [Serializable] struct BaseMonsterData
    {
        public int health;
        public int attack;
        public int exp;
        public string prefabName;
        public float speed;
        public float radius;
        public string deathAudioClip;
        public float deathVolume;
        public string deathVfx;
    }
    
    [Serializable] struct MonsterScaleData
    {
        public int id;
        public string baseId;
        public string skin;
        public float scaleRadius;
        public float scaleSpeed;
        public float stopDistance;
        public float healthScale;
        public float attackScale;
        public float expScale;
    }
}
