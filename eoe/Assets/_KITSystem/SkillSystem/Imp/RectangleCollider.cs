using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class RectangleCollider : BaseCollider
    {
        private readonly Vector2 rectangleSize;
        private readonly bool dependencyRelativeRotation;
        
        public RectangleCollider(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, Vector2 rectangleSize, bool dependencyRelativeRotation) : base(query, relativePosition, timerTrigger, duration)
        {
            this.rectangleSize = rectangleSize;
            this.dependencyRelativeRotation = dependencyRelativeRotation;
        }

        protected override List<int> OnCollision(Vector2 position, Vector2 direction)
        {
            return Query.GetAllEntities(GetPosition(position, direction), rectangleSize, direction, FilterEntity);
        }

        protected override void OnGizmos(Vector2 position, Vector2 direction, Color color, float deltaTime)
        {
#if UNITY_EDITOR
            Vector2 center = GetPosition(position, direction);
            
            Vector2 bl, br, tr, tl;
            
            if (dependencyRelativeRotation)
            {
                Vector2 right = new Vector2(direction.y, -direction.x);
                float halfForward = rectangleSize.x * 0.5f;
                float halfRight = rectangleSize.y * 0.5f;
                Vector2 hf = direction * halfForward;
                Vector2 hr = right * halfRight;

                bl = center - hf - hr;
                br = center - hf + hr;
                tr = center + hf + hr;
                tl = center + hf - hr;
            }
            else
            {
                Vector2 half = rectangleSize * 0.5f;
            
                bl = center + new Vector2(-half.x, -half.y); // Bottom Left
                br = center + new Vector2( half.x, -half.y); // Bottom Right
                tr = center + new Vector2( half.x,  half.y); // Top Right
                tl = center + new Vector2(-half.x,  half.y); // Top Left
            }

            Debug.DrawLine(bl, br, color, deltaTime);
            Debug.DrawLine(br, tr, color, deltaTime);
            Debug.DrawLine(tr, tl, color, deltaTime);
            Debug.DrawLine(tl, bl, color, deltaTime);

            Debug.DrawRay(position, direction * rectangleSize.magnitude, color, deltaTime);
#endif
        }
    }
}