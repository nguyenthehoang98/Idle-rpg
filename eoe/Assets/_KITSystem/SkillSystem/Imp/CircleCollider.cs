using System.Collections.Generic;
using _Game.GamePlay.Utils;
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
            Vector3 center = GetPosition(position, direction);
            
            GizmosLine.Circle(center, radius, color, deltaTime);

            Debug.DrawRay(position, direction * radius, color, deltaTime);
#endif
        }
    }
}