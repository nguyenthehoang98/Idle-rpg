using System;
using System.Collections.Generic;
using _KITSystem.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace _Game.Configs
{
#if UNITY_EDITOR
    [Serializable]
    // không dùng cho runtime
    public class SkillConfig : IGameConfig
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();
        [JsonProperty] private List<ColliderData> colliders = new List<ColliderData>();
        [JsonProperty] private List<TrajectoryData> trajectories = new List<TrajectoryData>();
        [JsonProperty] private List<FindTargetData> findTargets = new List<FindTargetData>();
        [JsonProperty] private List<DamageTickerData> damageTickers = new List<DamageTickerData>();

        private Dictionary<int, SkillData> cached;
        
        public void OnMappingValue()
        {
            cached = new Dictionary<int, SkillData>();

            foreach (var skillData in skills)
            {
                if (!cached.TryAdd(skillData.skillId, skillData)) Debug.LogError($"Duplicate skill '{skillData.skillId}'");
            }
        }

        public void OnPostImported()
        {
            for (int i = 0; i < skills.Count; i++)
            {
                SkillData skill = skills[i];

                bool found = false;
                foreach (var c in colliders)
                {
                    if (skill.colliderId == c.id)
                    {
                        skill.collider = c;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    Debug.LogError($"Not found skill at '{skill.skillId}', collider id '{skill.colliderId}'");

                found = false;
                foreach (var t in trajectories)
                {
                    if (skill.trajectoryId == t.id)
                    {
                        skill.trajectory = t;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    Debug.LogError($"Not found skill at '{skill.skillId}', trajectory id '{skill.trajectoryId}'");
                
                found = false;
                foreach (var f in findTargets)
                {
                    if (skill.findTargetId == f.id)
                    {
                        skill.findTarget = f;
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    Debug.LogError($"Not found skill at '{skill.skillId}', findTarget id '{skill.findTargetId}'");
                
                found = false;
                foreach (var d in damageTickers)
                {
                    if (skill.damageTickerId == d.id)
                    {
                        skill.damageTicker = d;
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    Debug.LogError($"Not found skill at '{skill.skillId}', damageTicker id '{skill.damageTickerId}'");
                

                skills[i] = skill;
            }
        }

        public void OnValidateLinkConfig()
        {
        }

        public bool TryGetSkillOnEditor(int skillId, out SkillData skill)
        {
            return cached.TryGetValue(skillId, out skill);
        }
    }    
#endif

    [Serializable]
    public struct SkillData
    {
        public int skillId;
        public float lifeTime;
        public string prefabName;
        
        [JsonProperty, NonSerialized] public string findTargetId;
        [JsonProperty, NonSerialized] public string trajectoryId;
        [JsonProperty, NonSerialized] public string colliderId;
        [JsonProperty, NonSerialized] public string damageTickerId;

        public FindTargetData findTarget;
        public TrajectoryData trajectory;
        public ColliderData collider;
        public DamageTickerData damageTicker;
    }

    [Serializable]
    public struct ColliderData
    {
        [NonSerialized] public string id;

        public float timerTrigger;
        public float duration;
        public int limitNumberCollision;
        public float resetCollisionInterval;
        public ColliderType type;
        public float radius;
        public Vector2 relativePosition;
    }

    [Serializable]
    public struct TrajectoryData
    {
        [NonSerialized] public string id;
        
        public TrajectoryType type;
        public float bulletInitSpeed;
        public float bulletAcceleration;
    }

    [Serializable]
    public struct FindTargetData
    {
        [NonSerialized] public string id;
        
        public FilterType type;
        public float radius;

        public enum FilterType
        {
            None = 0,
            Nearest,
            Farthest,
            HpLowest,
            HpHighest,
            AtkLowest,
            AtkHighest,
            DefLowest,
            DefHighest
        }
    }

    [Serializable]
    public struct DamageTickerData
    {
        [NonSerialized] public string id;
        
        public DamageTickerType type;
        public float ticketInterval;
    }
    
    public enum DamageTickerType
    {
        Instant, DamageOverTime
    }

    public enum TrajectoryType
    {
        Bullet = 1,
    }

    public enum ColliderType
    {
        Circle
    }
}