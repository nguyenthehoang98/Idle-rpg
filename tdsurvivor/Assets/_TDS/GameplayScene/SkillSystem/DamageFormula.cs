using _Toolkit.Shared;

namespace _TDS.GameplayScene.SkillSystem
{
    public static class DamageFormula
    {
        public static float DamageOutput(SkillContext context, out bool critical)
        {
            critical = RandomUtils.Value <= context.CritRate;
            return context.Attack * (1 + (critical ? context.CritDamage : 0));
        }
    }
}