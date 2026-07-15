using System;
using System.Collections.Generic;
using _KITSystem.Config;
using _KITSystem.SkillSystem.Core;
using _KITSystem.SkillSystem.Imp;
using UnityEngine;
using UnityEngine.Serialization;

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

            foreach (SkillData skillData in skills)
            {
                if (cached.TryAdd(skillData.skillId, skillData)) continue;

                Debug.LogError($"Duplicate skill '{skillData.skillId}'");
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
        /*
         * @Default stat
         */
        public int skillId;
        public string prefabName; // Đọc từ weapon. monster
        public float size;

        /*
         * @Damage ticet
         */
        public float damageInterval;

        /*
         * @Query entity
         */
        public FindTargetType findTarget;
        public float findRadius;

        /*
         * @Trajectory
         */
        public TrajectoryType trajectory;
        public float projectileSpeed;
        public float boomerangOutboundSpeed;
        public float boomerangOutboundDuration;
        public float boomerangReturnDuration;
        public float boomerangHangDuration;
        /*
         * @Collider
         */
        public float collTimerTrigger;
        public float collDuration;
        public float collResetCollision;
        public int collLimitCollision;

        /*
         * @Extra/bonus
         */
        public float spreadAngleStep;
        public float parallelDistanceStep;
        public string explosivePrefabName;
    }
}