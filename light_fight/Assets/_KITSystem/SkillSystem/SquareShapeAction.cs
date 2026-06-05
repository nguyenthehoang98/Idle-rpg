using System.Collections.Generic;
using _KITSystem.Entity;
using _KITSystem.SkillSystem.Model;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    internal class SquareShapeAction : BaseShapeAction
    {
        private readonly Vector2 size;
        
        public SquareShapeAction(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, Vector2 size) : base(query, relativePosition, timerTrigger, duration)
        {
            this.size = size;
        }

        protected override List<int> OnCollision(Vector2 position)
        {
            return Query.GetEntities(GetPosition(position), FilterEntity, size);
        }
        
        public override void Gizmos(Vector3 position, Color color, float deltaTime)
        {
            float2 f2 = GetPosition(new float2(position.x, position.y));
            Vector3 center = new Vector3(f2.x, f2.y, 0);
            Vector3 half = size / 2f;

            Vector3 topLeft = center + new Vector3(-half.x, half.y, 0);
            Vector3 topRight = center + new Vector3(half.x, half.y, 0);
            Vector3 bottomRight = center + new Vector3(half.x, -half.y, 0);
            Vector3 bottomLeft = center + new Vector3(-half.x, -half.y, 0);

            Debug.DrawLine(topLeft, topRight, color, deltaTime);
            Debug.DrawLine(topRight, bottomRight, color, deltaTime);
            Debug.DrawLine(bottomRight, bottomLeft, color, deltaTime);
            Debug.DrawLine(bottomLeft, topLeft, color, deltaTime);
        }
    }
}