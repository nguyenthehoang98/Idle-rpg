namespace _Games.Combat.SkillSystem.Model
{
    [System.Serializable]
    public sealed class SkillMainModule
    {
        public float lifeTime;
        public FindTargetType type;
        public bool needTargetToCast;
        public float maxTargetRange;
        public float collisionResetInterval;
    }
}