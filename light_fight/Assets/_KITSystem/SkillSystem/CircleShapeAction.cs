using System.Collections.Generic;
using _KITSystem.Entity;
using _KITSystem.SkillSystem.Model;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    internal class CircleShapeAction : BaseShapeAction
    {
        private float radius;
        public CircleShapeAction(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, float radius) : base(query, relativePosition, timerTrigger, duration)
        {
            this.radius = radius;
        }

        protected override List<int> OnCollision(Vector2 position)
        {
            return Query.GetEntities(GetPosition(position), FilterEntity, radius);
        }
        
        public override void Gizmos(Vector3 position, Color color, float deltaTime)
        {
            Vector2 center = GetPosition(new Vector2(position.x, position.y));
            int segments = 12;
            float angleStep = 360f / segments;
            Vector2 prevPoint = center + new Vector2(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i;
                Vector2 newPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Debug.DrawLine(new Vector3(prevPoint.x, prevPoint.y), new Vector3(newPoint.x, newPoint.y), color, deltaTime);
                prevPoint = newPoint;
            }
        }
    }
}