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
        [SerializeField] private int si;

        public int skillId
        {
            get => si;
            set => si = value;
        }

        [SerializeField] private string pn;

        public string prefabName // Đọc từ weapon. monster
        {
            get => pn;
            set => pn = value;
        }

        [SerializeField] private float s;

        public float size
        {
            get => s;
            set => s = value;
        }

        /*
         * @Damage ticet
         */
        [SerializeField] private float di;

        public float damageInterval
        {
            get => di;
            set => di = value;
        }

        /*
         * @Query entity
         */
        [SerializeField] private FindTargetType ft;

        public FindTargetType findTarget
        {
            get => ft;
            set => ft = value;
        }

        [SerializeField] private float fr;

        public float findRadius
        {
            get => fr;
            set => fr = value;
        }

        /*
         * @Trajectory
         */
        [SerializeField] private TrajectoryType t;

        public TrajectoryType trajectory
        {
            get => t;
            set => t = value;
        }

        [SerializeField] private float ps;

        public float projectileSpeed
        {
            get => ps;
            set => ps = value;
        }

        [SerializeField] private float bos;

        public float boomerangOutboundSpeed
        {
            get => bos;
            set => bos = value;
        }

        [SerializeField] private float bod;

        public float boomerangOutboundDuration
        {
            get => bod;
            set => bod = value;
        }

        [SerializeField] private float brd;

        public float boomerangReturnDuration
        {
            get => brd;
            set => brd = value;
        }

        [SerializeField] private float bhd;

        public float boomerangHangDuration
        {
            get => bhd;
            set => bhd = value;
        }

        /*
         * @Collider
         */
        [SerializeField] private float ctt;

        public float collTimerTrigger
        {
            get => ctt;
            set => ctt = value;
        }

        [SerializeField] private float cd;

        public float collDuration
        {
            get => cd;
            set => cd = value;
        }

        [SerializeField] private float crc;

        public float collResetCollision
        {
            get => crc;
            set => crc = value;
        }

        [SerializeField] private int clc;

        public int collLimitCollision
        {
            get => clc;
            set => clc = value;
        }

        /*
         * @Extra/bonus
         */
        [SerializeField] private float sas;

        public float spreadAngleStep
        {
            get => sas;
            set => sas = value;
        }

        [SerializeField] private float pds;

        public float parallelDistanceStep
        {
            get => pds;
            set => pds = value;
        }

        [SerializeField] private string epn;

        public string explosivePrefabName
        {
            get => epn;
            set => epn = value;
        }
    }
}