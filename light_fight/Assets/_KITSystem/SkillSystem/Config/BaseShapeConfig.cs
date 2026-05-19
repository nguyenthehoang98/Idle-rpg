using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseShapeConfig
    {
        [TitleGroup("$Type")] 
        [Indent] public float triggerTimeInSeconds;
        [Indent] public float2 offsetRelativePosition;
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