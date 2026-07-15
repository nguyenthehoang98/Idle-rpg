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

        protected Vector2 GetPosition(Vector2 prevPosition, Vector2 currentPosition)
        {
            Vector2 direction = (currentPosition - prevPosition).normalized;

            if (direction.sqrMagnitude < Mathf.Epsilon)
                return currentPosition + relativePosition;

            float angle = Mathf.Atan2(direction.y, direction.x);

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector2 rotatedOffset = new Vector2(
                relativePosition.x * cos - relativePosition.y * sin,
                relativePosition.x * sin + relativePosition.y * cos
            );

            return currentPosition + rotatedOffset;
        }

        protected bool FilterEntity(int entity) => true;

        public void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            canTrigger = timerTrigger <= elapsedTime && elapsedTime <= duration + timerTrigger;
        }

        public List<int> Collision(Vector2 prevPosition, Vector2 currentPosition)
        {
            if (canTrigger)
            {
                return OnCollision(prevPosition, currentPosition);
            }

            return null;
        }

        protected abstract List<int> OnCollision(Vector2 prevPosition, Vector2 currentPosition);

        public void Gizmos(Vector2 prevPosition, Vector3 currentPosition, Color color, float deltaTime)
        {
            if(canTrigger) OnGizmos(prevPosition, currentPosition, color, deltaTime);
        }

        protected virtual void OnGizmos(Vector2 prevPosition, Vector3 currentPosition, Color color, float deltaTime)
        {
        }
    }
}