using System;

namespace _KITSystem.SkillSystem.Trigger
{
    [Serializable]
    public class TimelineTrigger : BaseTrigger
    {
        public float timer;
        public override TriggerType Type => TriggerType.Timeline;
    }
}