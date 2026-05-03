using System;

namespace _KITSystem.SkillSystem.Config.Trigger
{
    [Serializable]
    public class EventBasedTrigger : BaseTrigger
    {
        public int id;
        public override TriggerType Type => TriggerType.Event;
    }
}