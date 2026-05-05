using System;
using Sirenix.OdinInspector;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class ConeShapeConfig : BaseShapeConfig
    {
        [Indent] public float widthTop;
        [Indent] public float widthBottom;
        [Indent] public float height;
        [Indent] public int n;
        public override ShapeType Type => ShapeType.Cone;
    }
}