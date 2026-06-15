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
        
        public void OnMappingValue()
        {
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
                    if (b.id == data.base_id)
                    {
                        monsters.Add(new MonsterData
                        {
                            id = b.id,
                            prefabName = b.prefab_name,
                            speed = b.speed * data.scale_speed,
                            radius = b.radius * data.scale_radius,
                            color = data.color
                        });
                        found = true;
                        break;
                    }
                }

                if (!found) Debug.LogError($"Not found monster data with id '{data.id}', base '{data.base_id}'");
            }
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
        public string prefab_name;
        public float speed;
        public float radius;
    }
    
    [Serializable] struct MonsterScaleData
    {
        public int id;
        public int base_id;
        public float scale_radius;
        public float scale_speed;
        [JsonProperty] private string hex;
        public Color color;

        public void OnImported()
        {
            ColorUtility.TryParseHtmlString(hex, out color);
        }
    }
}
