using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class SquareShapeAction : BaseShapeAction
    {
        private Vector2 size;
        SquareShape.PivotType pivotType;
            
        public SquareShapeAction(SquareShape shape) : base(shape)
        {
            size = shape.size;
            pivotType = shape.pivotType;
        }

        protected override bool OnHit(Vector3 position, out List<int> hitsId)
        {
            Vector3 center = GetPosition(position) - GetPivotToCenterOffset(size, pivotType);
            // todo: scan objects
            hitsId = null;
            return false;
        }

        public override void Gizmos(Vector3 position, Vector3 goal, Color color, float duration)
        {
            Vector3 center = GetPosition(position) - GetPivotToCenterOffset(size, pivotType);
            Vector2 half = size / 2f;
            
            Vector3 topLeft     = center + new Vector3(-half.x,  half.y, 0);
            Vector3 topRight    = center + new Vector3( half.x,  half.y, 0);
            Vector3 bottomRight = center + new Vector3( half.x, -half.y, 0);
            Vector3 bottomLeft  = center + new Vector3(-half.x, -half.y, 0);

            Debug.DrawLine(topLeft, topRight, color, duration);
            Debug.DrawLine(topRight, bottomRight, color, duration);
            Debug.DrawLine(bottomRight, bottomLeft, color, duration);
            Debug.DrawLine(bottomLeft, topLeft, color, duration);
        }

        static Vector3 GetPivotToCenterOffset(Vector2 size, SquareShape.PivotType pivot)
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