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

        [SerializeField] private string s;

        public string skin
        {
            get => s;
            set => s = value;
        }

        [SerializeField] private float s2;

        public float speed
        {
            get => s2;
            set => s2 = value;
        }

        [SerializeField] private float s3;

        public float scale
        {
            get => s3;
            set => s3 = value;
        }

        [SerializeField] private float r;

        public float radius
        {
            get => r;
            set => r = value;
        }

        [SerializeField] private float sd;

        public float stopDistance
        {
            get => sd;
            set => sd = value;
        }

        [SerializeField] private string mp3;

        public string deathAudioClip
        {
            get => mp3;
            set => mp3 = value;
        }

        [SerializeField] private float dv;

        public float deathVolume
        {
            get => dv;
            set => dv = value;
        }

        [SerializeField] private string dvfx;

        public string deathVfx
        {
            get => dvfx;
            set => dvfx = value;
        }

        [SerializeField] private float a;

        public float attack
        {
            get => a;
            set => a = value;
        }

        [SerializeField] private float h2;

        public float health
        {
            get => h2;
            set => h2 = value;
        }

        [SerializeField] private float e2;

        public float exp
        {
            get => e2;
            set => e2 = value;
        }
    }

    [Serializable]
    struct BaseMonsterData
    {
        public int health { get; set; }
        public int attack { get; set; }
        public int exp { get; set; }
        public string prefabName { get; set; }
        public float speed { get; set; }
        public float radius { get; set; }
        public string deathAudioClip { get; set; }
        public float deathVolume { get; set; }
        public string deathVfx { get; set; }
    }

    [Serializable]
    struct MonsterScaleData
    {
        public int id { get; set; }
        public string baseId { get; set; }
        public string skin { get; set; }
        public float scaleRadius { get; set; }
        public float scaleSpeed { get; set; }
        public float stopDistance { get; set; }
        public float healthScale { get; set; }
        public float attackScale { get; set; }
        public float expScale { get; set; }
    }
}
