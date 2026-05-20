using System;
using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime.Signal;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class SquareShapeAction : BaseShapeAction
    {
        private Vector2 size;
        SquareShapeConfig.PivotType pivotType;
            
        public SquareShapeAction(SquareShapeConfig shapeConfig) : base(shapeConfig)
        {
            size = shapeConfig.size;
            pivotType = shapeConfig.pivotType;
        }
        
        protected override void OnHit(float2 position, Action<List<int>> callback)
        {
            float2 center = GetPosition(position) - GetPivotToCenterOffset(size, pivotType);
            SystemBus.Publish(new SquareShapeHitSignal(center, size, callback));
        }

        public override void Gizmos(Vector3 position, Color color, float duration)
        {
            float2 f2 = GetPosition(new float2(position.x, position.y)) - GetPivotToCenterOffset(size, pivotType);
            Vector3 center = new Vector3(f2.x, f2.y, 0);
            Vector3 half = size / 2f;

            Vector3 topLeft = center + new Vector3(-half.x, half.y, 0);
            Vector3 topRight = center + new Vector3(half.x, half.y, 0);
            Vector3 bottomRight = center + new Vector3(half.x, -half.y, 0);
            Vector3 bottomLeft = center + new Vector3(-half.x, -half.y, 0);

            Debug.DrawLine(topLeft, topRight, color, duration);
            Debug.DrawLine(topRight, bottomRight, color, duration);
            Debug.DrawLine(bottomRight, bottomLeft, color, duration);
            Debug.DrawLine(bottomLeft, topLeft, color, duration);
        }

        static float2 GetPivotToCenterOffset(Vector2 size, SquareShapeConfig.PivotType pivot)
        {
            Vector2 half = size * 0.5f;

            switch (pivot)
            {
                case SquareShapeConfig.PivotType.Center: return Vector2.zero;
                case SquareShapeConfig.PivotType.BottomLeft: return new Vector2(half.x, half.y);
                case SquareShapeConfig.PivotType.BottomRight: return new Vector2(-half.x, half.y);
                case SquareShapeConfig.PivotType.TopLeft: return new Vector2(half.x, -half.y);
                case SquareShapeConfig.PivotType.TopRight: return new Vector2(-half.x, -half.y);
            }

            return Vector2.zero;
        }
    }
}