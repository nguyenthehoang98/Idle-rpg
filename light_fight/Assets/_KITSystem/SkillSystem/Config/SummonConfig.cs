using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class SummonConfig : BaseActionConfig
    {
        public int monsterId;
        public int monsterLevel;
        
        public override ActionType Type => ActionType.SummonUnit;

        public override float Duration => 0;
    }
}