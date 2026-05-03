using System;

namespace _KITSystem.SkillSystem.Config.Trigger
{
    [Serializable]
    public class TimelineTrigger : BaseTrigger
    {
        public float timer;
        public override TriggerType Type => TriggerType.Timeline;
    }
}