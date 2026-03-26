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
        [SerializeField] private float baseFlatDamage;
        [SerializeField] private float flatDamageBonusLevel;
        [SerializeField] private float baseScaleDamage;
        [SerializeField] private float scaleDamageBonusLevel;
        [SerializeField] private FindTargetType type;
        [SerializeField] private int needTargetToCast;
        [SerializeField] private float maxFindTargetRange;
        [SerializeField] private int maxHitCount;
        [SerializeField] private float collisionResetInterval;
        [SerializeField] private string projectileId;
        [SerializeField] private string colliderId;
        [SerializeField] private string trajectoryId;
        [SerializeField] private string[] modifiersId = new string[0];
        [SerializeField] private string[] behaviorsId = new string[0];

        public int SkillId => skillId;
        public string SkillName => skillName;
        public float LifeTime => lifeTime;
        public float CastTime => castTime;
        public FindTargetType Type => type;
        public bool NeedTargetToCast => needTargetToCast == 1;
        public float MaxFindTargetRange => maxFindTargetRange;
        public int MaxHitCount => maxHitCount;
        public float CollisionResetInterval => collisionResetInterval;
        public string ProjectileId => projectileId;
        public string ColliderId => colliderId;
        public string TrajectoryId => trajectoryId;
        public string[] ModifiersId => modifiersId;
        public string[] BehaviorsId => behaviorsId;
        
        public float FlatDamage(int level) => baseFlatDamage + level * flatDamageBonusLevel;
        public float ScaleDamage(int level) => baseScaleDamage + level * scaleDamageBonusLevel;
    }
}