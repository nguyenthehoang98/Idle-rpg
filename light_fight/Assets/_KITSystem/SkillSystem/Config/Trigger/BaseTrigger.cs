using System;

namespace _KITSystem.SkillSystem.Config.Trigger
{
    [Serializable]
    public abstract class BaseTrigger
    {
        public abstract TriggerType Type { get; }
        
        public enum TriggerType
        {
            Timeline,
            Event,
            Manual
        }
    }
}