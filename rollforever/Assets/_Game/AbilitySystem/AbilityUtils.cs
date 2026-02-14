using _Game.Battle;

namespace _Game.AbilitySystem
{
    public static class AbilityUtils
    {
        public static string SkillPath(this SkillId skillId)
        {
            return string.Format($"Skill_{skillId.id}{skillId.level}");
        }
    }
}