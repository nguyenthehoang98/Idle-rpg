using UnityEngine;

namespace _Games.Combat.SkillSystem.Model
{
    [System.Serializable]
    public class SkillData
    {
        [SerializeField] private int skillId;
        [SerializeField] private string skillName;
        [SerializeField] private float lifeTime;
        [SerializeField] private float castTime;
        [SerializeField] private FindTargetType type;
        [SerializeField] private bool needTargetToCast;
        [SerializeField] private float maxTargetRange;
        [SerializeField] private int maxHitCount;
        [SerializeField] private float collisionResetInterval;
        [SerializeField] private string projectileId;
        [SerializeField] private string colliderId;
        [SerializeField] private string trajectoryId;
        [SerializeField] private string[] modifiersId;
        [SerializeField] private string[] behaviorsId;

        public int SkillId => skillId;
        public string SkillName => skillName;
        public float LifeTime => lifeTime;
        public float CastTime => castTime;
        public FindTargetType Type => type;
        public bool NeedTargetToCast => needTargetToCast;
        public float MaxTargetRange => maxTargetRange;
        public int MaxHitCount => maxHitCount;
        public float CollisionResetInterval => collisionResetInterval;
        public string ProjectileId => projectileId;
        public string ColliderId => colliderId;
        public string TrajectoryId => trajectoryId;
        public string[] ModifiersId => modifiersId;
        public string[] BehaviorsId => behaviorsId;
    }

    [System.Serializable]
    public class SkillStatData
    {
        [SerializeField] private int skillId;
        [SerializeField] private int skillLevel;
        [SerializeField] private float baseDamage;
        [SerializeField] private float scaleDamage;

        public int SkillId => skillId;
        public int SkillLevel => skillLevel;
        public float BaseDamage => baseDamage;
        public float ScaleDamage => scaleDamage;
    }
}