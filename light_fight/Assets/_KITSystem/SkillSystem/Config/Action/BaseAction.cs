using System;

namespace _KITSystem.SkillSystem.Config.Action
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