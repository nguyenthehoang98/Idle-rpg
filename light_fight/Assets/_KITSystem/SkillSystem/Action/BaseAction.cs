using System;

namespace _KITSystem.SkillSystem.Action
{
    [Serializable]
    public abstract class BaseAction
    {
        public abstract ActionType Type { get; }

        public enum ActionType
        {
            CastProjectile
        }
    }
}