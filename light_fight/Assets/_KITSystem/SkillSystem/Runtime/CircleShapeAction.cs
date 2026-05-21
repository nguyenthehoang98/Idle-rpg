using System;
using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime.Signal;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class CircleShapeAction : BaseShapeAction
    {
        private float radius;

        public CircleShapeAction(CircleShapeConfig circle) : base(circle)
        {
            radius = circle.radius;
        }

        protected override void OnHit(float2 position, Action<List<int>> callback)
        {
            float2 center = GetPosition(position);
            SystemBus.Publish(new CircleShapeHitEntitySignal(center, radius, callback));
        }

        public override void Gizmos(Vector3 position, Color color, float duration)
        {
            float2 center = GetPosition(new float2(position.x, position.y));
            int segments = 12;
            float angleStep = 360f / segments;
            float2 prevPoint = center + new float2(math.cos(0f), math.sin(0f)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i;
                float2 newPoint = center + new float2(math.cos(angle), math.sin(angle)) * radius;
                Debug.DrawLine(new Vector3(prevPoint.x, prevPoint.y), new Vector3(newPoint.x, newPoint.y), color, duration);
                prevPoint = newPoint;
            }
        }
    }
}