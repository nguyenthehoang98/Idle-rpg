using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseActionConfig
    {
        public abstract ActionType Type { get; }

        public enum ActionType
        {
            CastProjectile
        }
    }
}