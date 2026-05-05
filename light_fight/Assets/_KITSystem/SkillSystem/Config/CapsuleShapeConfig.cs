using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CapsuleShapeConfig : BaseShapeConfig
    {
        [Indent] public float radius = 1;
        [Indent] public float height = 2;
        public override ShapeType Type => ShapeType.Capsule;
    }
}