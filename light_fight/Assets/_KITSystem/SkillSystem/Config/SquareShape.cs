using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class SquareShape : BaseShape
    {
        [Indent] public Vector2 size = new Vector2(1, 1);
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