using System.Collections.Generic;
using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class RectangleCollider : BaseCollider
    {
        private readonly Vector2 size;
        
        public RectangleCollider(IQuery query, Vector2 relativePosition, float timerTrigger, float duration, Vector2 size) : base(query, relativePosition, timerTrigger, duration)
        {
            this.size = size;
        }

        protected override List<int> OnCollision(Vector2 prevPosition, Vector2 currentPosition)
        {
            return Query.GetAllEntities(GetPosition(prevPosition, currentPosition), (currentPosition - prevPosition).normalized, size, FilterEntity);
        }

        protected override void OnGizmos(Vector2 prevPosition, Vector2 currentPosition, Color color, float deltaTime)
        {
#if UNITY_EDITOR
            Debug.Log($"OnGizmos: {prevPosition} -> {currentPosition}");
            Vector2 direction = (currentPosition - prevPosition).normalized;
            Vector2 position = currentPosition;

            Vector2 right = direction * (size.x * 0.5f);
            Vector2 up = new Vector2(-direction.y, direction.x) * (size.y * 0.5f);

            Vector2 bl = position - right - up;
            Vector2 tl = position - right + up;
            Vector2 tr = position + right + up;
            Vector2 br = position + right - up;

            Debug.DrawLine(bl, tl, color, deltaTime);
            Debug.DrawLine(tl, tr, color, deltaTime);
            Debug.DrawLine(tr, br, color, deltaTime);
            Debug.DrawLine(br, bl, color, deltaTime);
#endif
        }
    }
}