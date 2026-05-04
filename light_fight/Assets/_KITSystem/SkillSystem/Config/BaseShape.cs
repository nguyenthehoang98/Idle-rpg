using System;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseShape
    {
        public float triggerTimeInSeconds;
        public abstract ShapeType Type { get; }

        public enum ShapeType
        {
            Square,
            Circle,
            Capsule,
            Cone
        }
    }
}