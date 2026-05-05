using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class CircleShapeAction : BaseShapeAction
    {
        private float radius;

        public CircleShapeAction(CircleShape circle) : base(circle)
        {
            radius = circle.radius;
        }

        protected override bool OnHit(Vector3 position, out List<int> hitsId)
        {
            Vector3 center = GetPosition(position);
            // todo: scan objects
            hitsId = null;
            return false;
        }

        public override void Gizmos(Vector3 position, Vector3 goal, Color color, float duration)
        {
            Vector3 center = GetPosition(position);
            int segments = 12;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Debug.DrawLine(prevPoint, newPoint, color, duration);
                prevPoint = newPoint;
            }
        }
    }
}