using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CircleShapeConfig : BaseShapeConfig
    {
        [Indent] public float radius = 1f;
        public override ShapeType Type => ShapeType.Circle;
    }
}