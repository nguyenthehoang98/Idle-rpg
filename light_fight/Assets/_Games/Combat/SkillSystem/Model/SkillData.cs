namespace _Games.Combat.SkillSystem.Model
{
    [System.Serializable]
    public class SkillData
    {
        public string skillId;
        public string skillName;
        public float lifeTime;
        public float castTime;
        public FindTargetType type;
        public bool needTargetToCast;
        public float maxTargetRange;
        public int maxHitCount;
        public float collisionResetInterval;
        public string projectileId;
        public string colliderId;
        public string trajectoryId;
        public string[] modifiersId;
        public string[] behaviorsId;
    }
}