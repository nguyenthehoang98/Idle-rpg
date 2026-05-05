using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public abstract class BaseShapeConfig
    {
        [TitleGroup("$Type")] 
        [Indent] public float triggerTimeInSeconds;
        [Indent] public Vector2 offsetRelativePosition;
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