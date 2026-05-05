using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class TriggerConfig
    {
        public TriggerType type;
        [ShowIf("type", TriggerType.Event)]
        public int eventId;
        [ShowIf("type", TriggerType.Event)]
        public bool isMultiplierTrigger = false;
        [ShowIf("type", TriggerType.Timeline)]
        public float timer;
        
        public enum TriggerType
        {
            Timeline,
            Event,
        }
    }
}