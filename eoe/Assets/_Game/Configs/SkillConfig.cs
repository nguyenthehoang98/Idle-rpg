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
                if (!cached.TryAdd(skillData.skillId, skillData))
                {
                    Debug.LogError($"Duplicate skill '{skillData.skillId}'");
                }
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

        /*
         * @Damage ticet
         */
        public float damageTickInterval;

        /*
         * @Query entity
         */
        public FindTargetType findTarget;
        public float attackRange;

        /*
         * @Trajectory
         */
        public TrajectoryType trajectory;
        public float projectileDuration;
        public float boomerangOutboundDuration;
        public float boomerangReturnDuration;
        public float boomerangHangDuration;
        public TrajectoryStationaryPivot stationaryPivot;
        public float stationaryRandomRadius;
        public float stationaryDuration;
        public float splineWindupDuration;
        public float splineExecuteDuration;
        public float splineRecoveryDuration;
        /*
         * @Collider
         */
        public float collisionStartDelay;
        public float collisionDuration;
        public float targetHitCooldown;
        public int maxHitCount;

        /*
         * @Extra/bonus
         */
        public float spreadAngleStep;
        public float parallelDistanceStep;
    }

    public enum TrajectoryStationaryPivot
    {
        Weapon, Enemy, Random
    }
}