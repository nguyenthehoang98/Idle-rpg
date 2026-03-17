using UnityEngine;

namespace _Games.Combat.SkillSystem.Model
{
    [System.Serializable]
    public sealed class SkillMainModule
    {
        public Sprite icon;
        public float lifeTime;
        public float castTime;
        public FindTargetType type;
        public bool needTargetToCast;
        public float maxTargetRange;
        public int maxHitCount;
        public float collisionResetInterval;
    }
}