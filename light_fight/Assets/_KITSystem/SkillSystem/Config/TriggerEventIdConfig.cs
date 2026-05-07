using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class TriggerEventIdConfig : BaseActionConfig
    {
        public int eventId = -1;
        
        public override ActionType Type => ActionType.TriggerEventId;
        public override float Duration => 0;
    }
}