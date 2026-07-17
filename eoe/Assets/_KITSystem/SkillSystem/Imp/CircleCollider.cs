using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class CircleCollider : BaseCollider
    {
        private readonly Vector2 size;
        private readonly float radius;
        
        public CircleCollider(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, float radius) : base(query, relativePosition, timerTrigger, duration)
        {
            this.radius = radius;
            
            this.size = new Vector2(radius / 2f, radius / 2f);
        }

        protected override List<int> OnCollision(Vector2 position, Vector2 direction)
        {
            return Query.GetAllEntities(GetPosition(position, direction), size, FilterEntity);
        }
        
        protected override void OnGizmos(Vector2 position, Vector2 direction, Color color, float deltaTime)
        {
#if UNITY_EDITOR
            int segments = 12;
            
            Vector3 center = GetPosition(position, direction);
            
            Vector3 prev = center + Vector3.right * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                
                float angle = t * Mathf.PI * 2f;
                
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                
                Debug.DrawLine(prev, next, color, deltaTime);
                
                prev = next;
            }

            Debug.DrawRay(position, direction * radius, color, deltaTime);
#endif
        }
    }
}