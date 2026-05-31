using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [System.Serializable]
    public class DefaultSkillConfig
    {
        [Tooltip("Thời gian sống của toàn bộ kĩ năng")]
        public float lifeTimeInSeconds = 1f;

        // Không dùng cho phiên bản này
        /*[Tooltip("Thời gian khóa hành vi kĩ năng, trong thời gian này kĩ năng được niệm và không nhận input")]
        public float channelingExitTimeInSeconds = 0f;
        public string animationName;
        public float scaleTime = 1f;*/

        [TitleGroup("Find Target")] 
        public TargetType targetType = TargetType.Nearest;
        public float radiusScanTarget = 1;

        public virtual SkillType Type => SkillType.Default;
    }

    public enum TargetType
    {
        Random,
        Nearest,
        Farthest,
        HealthLowest,
        HealthHighest,
        DamageLowest,
        DamageHighest,
    }

    public enum SkillType
    {
        Default,
    }
}