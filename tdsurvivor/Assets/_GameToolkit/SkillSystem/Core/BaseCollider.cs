using System.Collections.Generic;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    public abstract class BaseCollider
    {
        protected readonly IQuery Query;
        
        private readonly float collisionStartDelay;
        private readonly float duration;
        private readonly Vector2 relativePosition;
        
        private float elapsedTime;
        
        private bool canTrigger;

        // Trả về chung khi chưa canTrigger để tránh null-check/alloc mỗi tick
        private static readonly List<int> EmptyResult = new List<int>();

        protected BaseCollider(IQuery query, Vector2 relativePosition, float collisionStartDelay, float duration)
        {
            Query = query;
            this.collisionStartDelay = collisionStartDelay;
            this.duration = duration <= 0 ? float.MaxValue : duration;
            this.relativePosition = relativePosition;
        }

        protected Vector2 GetPosition(Vector2 position, Vector2 direction)
        {
            if (direction.sqrMagnitude < Mathf.Epsilon)
                return position + relativePosition;

            float angle = Mathf.Atan2(direction.y, direction.x);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector2 rotatedOffset = new Vector2(
                relativePosition.x * cos - relativePosition.y * sin,
                relativePosition.x * sin + relativePosition.y * cos
            );

            return position + rotatedOffset;
        }

        protected virtual bool FilterEntity(int entity) => true;

        public void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            
            canTrigger = collisionStartDelay <= elapsedTime && elapsedTime <= duration + collisionStartDelay;
        }

        public List<int> Collision(Vector2 position, Vector2 direction)
        {
            if (canTrigger)
            {
                return OnCollision(position, direction);
            }

            return EmptyResult;
        }

        protected abstract List<int> OnCollision(Vector2 position, Vector2 direction);

        public void Gizmos(Vector2 position, Vector2 direction, Color color, float deltaTime)
        {
            if(canTrigger) OnGizmos(position, direction, color, deltaTime);
        }

        protected virtual void OnGizmos(Vector2 position, Vector2 direction, Color color, float deltaTime)
        {
        }
    }
}