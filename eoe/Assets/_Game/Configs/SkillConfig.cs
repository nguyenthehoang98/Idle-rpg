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
        public string prefabName; // Đọc từ weapon. monster
        
        public DamageTickerType tickerType;
        public float ticketInterval;
        
        public FilterType filterType;
        public float filterRadius;
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
        
        public float collTimerTrigger;
        public float collDuration;
        public float collResetCollision;
        public int collLimitCollision;
        
        public TrajectoryType trajectoryType;
        public float bulletInitSpeed;
        public float bulletAcceleration;
        
        public float spreadAngleStep;
        public float parallelDistanceStep;
        public string explosivePrefabName;
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