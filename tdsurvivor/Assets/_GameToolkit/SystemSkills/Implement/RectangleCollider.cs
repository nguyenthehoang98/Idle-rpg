using System.Collections.Generic;
using _Game.GamePlay.Utils;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class RectangleCollider : BaseCollider
    {
        private readonly Vector2 rectangleSize;
        private readonly bool dependencyRelativeRotation;
        
        public RectangleCollider(IQuery query, Vector2 relativePosition, float collisionStartDelay, float duration, Vector2 rectangleSize, bool dependencyRelativeRotation) : base(query, relativePosition, collisionStartDelay, duration)
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
            
            if (dependencyRelativeRotation)
            {
                GizmosLine.Rectangle(center, direction, rectangleSize, color, deltaTime);
            }
            else
            {
                GizmosLine.Rectangle(center, rectangleSize, color, deltaTime);
            }

            Debug.DrawRay(position, direction * rectangleSize.magnitude, color, deltaTime);
#endif
        }
    }
}