using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.SkillSystem.Core
{
    public abstract class BaseCollider
    {
        protected readonly IQuery Query;
        
        private readonly float timerTrigger;
        private readonly float duration;
        private readonly Vector2 relativePosition;
        
        private float elapsedTime;
        private bool canTrigger;

        protected BaseCollider(IQuery query, Vector2 relativePosition, float timerTrigger, float duration)
        {
            Query = query;
            this.timerTrigger = timerTrigger;
            this.duration = duration;
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

        protected bool FilterEntity(int entity) => true;

        public void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            canTrigger = timerTrigger <= elapsedTime && elapsedTime <= duration + timerTrigger;
        }

        public List<int> Collision(Vector2 position, Vector2 direction)
        {
            if (canTrigger)
            {
                return OnCollision(position, direction);
            }

            return null;
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