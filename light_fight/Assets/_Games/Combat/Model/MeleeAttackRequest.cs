using Unity.Entities;

namespace _Games.Combat.Model
{
    public readonly struct MeleeAttackRequest
    {
        public readonly Entity Target;
        public readonly int SkillId;
        public readonly int SkillLevel;
        public readonly int MonsterId;
        public readonly bool IsValid;

        public MeleeAttackRequest(Entity target, int monsterId, int skillId, int skillLevel)
        {
            Target = target;
            SkillId = skillId;
            SkillLevel = skillLevel;
            MonsterId = monsterId;
            IsValid = true;
        }

        public static MeleeAttackRequest None()
        {
            return new MeleeAttackRequest();
        }
    }
}