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
        [JsonProperty] private List<BaseMonsterData>  monster_base = new List<BaseMonsterData>();

        private Dictionary<int, MonsterData> cached;
        
        public void OnMappingValue()
        {
            cached = new Dictionary<int, MonsterData>();

            foreach (var monsterData in monsters)
            {
                cached.Add(monsterData.id, monsterData);
            }
        }

        public void OnPostImported()
        {
            for (var i = 0; i < monster_scale.Count; i++)
            {
                var data = monster_scale[i];
                data.OnImported();
                monster_scale[i] = data;
            }

            monsters = new List<MonsterData>();
            for (int i = 0; i < monster_scale.Count; i++)
            {
                var data = monster_scale[i];

                bool found = false;
                foreach (var b in monster_base)
                {
                    if (b.id == data.baseId)
                    {
                        monsters.Add(new MonsterData
                        {
                            id = b.id,
                            prefabName = b.prefabName,
                            speed = b.speed * data.scaleSpeed,
                            radius = b.radius * data.scaleRadius,
                            color = data.color
                        });
                        found = true;
                        break;
                    }
                }

                if (!found) Debug.LogError($"Not found monster data with id '{data.id}', base '{data.baseId}'");
            }
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
        public float speed;
        public float radius;
        public Color color;
    }

    [Serializable] struct BaseMonsterData
    {
        public int id;
        public string prefabName;
        public float speed;
        public float radius;
    }
    
    [Serializable] struct MonsterScaleData
    {
        public int id;
        public int baseId;
        public float scaleRadius;
        public float scaleSpeed;
        [JsonProperty] private string hex;
        public Color color;

        public void OnImported()
        {
            ColorUtility.TryParseHtmlString(hex, out color);
        }
    }
}
