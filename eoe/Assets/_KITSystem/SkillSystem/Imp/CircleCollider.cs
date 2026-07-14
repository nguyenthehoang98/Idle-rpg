using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class CircleCollider : BaseCollider
    {
        private Vector2 size;
        private float radius;
        
        public CircleCollider(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, float radius) : base(query, relativePosition, timerTrigger, duration)
        {
            this.radius = radius;
            this.size = new Vector2(radius / 2f, radius / 2f);
        }

        protected override List<int> OnCollision(Vector2 position)
        {
            return Query.GetAllEntities(GetPosition(position), size, FilterEntity);
        }
        
        public override void Gizmos(Vector3 position, Color color, float deltaTime)
        {
#if UNITY_EDITOR
            int segments = 12;
            Vector3 center = position;
            Vector3 prev = center + Vector3.right * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Debug.DrawLine(prev, next, color, deltaTime);
                prev = next;
            }
#endif
        }
    }
}