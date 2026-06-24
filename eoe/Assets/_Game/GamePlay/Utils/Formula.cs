using _Game.GamePlay.Data;
using _KITSystem.Utils;

namespace _Game.GamePlay.Utils
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