using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class SquareShapeConfig : BaseShapeConfig
    {
        [Indent] public float2 size = new(1, 1);
       
        public override ShapeType Type => ShapeType.Square;
       
        [Indent] public PivotType pivotType;

        public enum PivotType
        {
            Center,
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight,
        }
    }
}