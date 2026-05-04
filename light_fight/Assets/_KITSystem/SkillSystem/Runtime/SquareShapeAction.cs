using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class SquareShapeAction : BaseShapeAction
    {
        private Vector2 size;
        private Vector2 pivotRelativePosition;
        SquareShape.PivotType pivotType;
            
        public SquareShapeAction(SquareShape square) : base(square)
        {
            size = square.size;
            pivotType = square.pivotType;
            pivotRelativePosition = square.pivotRelativePosition;
        }

        protected override bool OnHit(Vector3 position, out List<OwnGameObject> targets)
        {
            Vector2 realPosition = (Vector2)position + pivotRelativePosition + GetPivotToCenterOffset(size, pivotType);
            // todo: scan objects
            targets = null;
            return false;
        }

        static Vector2 GetPivotToCenterOffset(Vector2 size, SquareShape.PivotType pivot)
        {
            Vector2 half = size * 0.5f;

            switch (pivot)
            {
                case SquareShape.PivotType.Center: return Vector2.zero;
                case SquareShape.PivotType.BottomLeft: return new Vector2(half.x, half.y);
                case SquareShape.PivotType.BottomRight: return new Vector2(-half.x, half.y);
                case SquareShape.PivotType.TopLeft: return new Vector2(half.x, -half.y);
                case SquareShape.PivotType.TopRight: return new Vector2(-half.x, -half.y);
            }

            return Vector2.zero;
        }
    }
}