using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class SquareShape : BaseShape
    {
        [TitleGroup("", "Square")] public Vector2 size = new Vector2(1, 1);
        [TitleGroup("", "Square")] public Vector3 pivotRelativePosition;
        public override ShapeType Type => ShapeType.Square;
        [TitleGroup("", "Box")] public PivotType pivotType;

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