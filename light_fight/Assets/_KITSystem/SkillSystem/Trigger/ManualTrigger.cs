using System;

namespace _KITSystem.SkillSystem.Trigger
{
    [Serializable]
    public class ManualTrigger : BaseTrigger
    {
        public string cookie;
        public override TriggerType Type => TriggerType.Manual;
    }
}