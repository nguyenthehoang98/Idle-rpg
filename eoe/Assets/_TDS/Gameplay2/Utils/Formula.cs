using _GameToolkit.Share;
using _TDS.Gameplay.Data;

namespace _TDS.Gameplay.Utils
{
    public static class Formula
    {
        public static float CalculateFinalDamage(SkillRuntimeData runtimeData, out bool critical)
        {
            critical = RandomUtils.Value <= runtimeData.CritChance;
            return runtimeData.Attack * (1 + (critical ? runtimeData.CritDamage : 0));
        }
    }
}