using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.SkillSystem;
using UnityEngine;

namespace _TDS.GameConfig
{
#if UNITY_EDITOR
    [Serializable]
    public class SkillConfig : IConfig
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

        public void OnImported()
        {
        }

        public void OnCompleteImported()
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
        public string prefabName;

        public float damageTickInterval;

        public FindTargetType findTarget;
        public float attackRange;

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

        public float collisionStartDelay;
        public float collisionDuration;
        public float targetHitCooldown;
        public int maxHitCount;

        public float spreadAngleStep;
        public float parallelDistanceStep;
    }

    public enum TrajectoryStationaryPivot
    {
        Weapon, Enemy, Random
    }
}
