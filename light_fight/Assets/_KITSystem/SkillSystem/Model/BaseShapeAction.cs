using System.Collections.Generic;
using _KITSystem.Entity;
using UnityEngine;

namespace _KITSystem.SkillSystem.Model
{
    internal abstract class BaseShapeAction
    {
        protected readonly IQuery Query;
        private readonly float timerTrigger;
        private readonly float duration;
        private readonly Vector2 relativePosition;
        
        private float elapsedTime;
        private bool canTrigger;

        protected BaseShapeAction(IQuery query, Vector2 relativePosition, float timerTrigger, float duration)
        {
            Query = query;
            this.timerTrigger = timerTrigger;
            this.duration = duration;
            this.relativePosition = relativePosition;
        }

        protected Vector2 GetPosition(Vector2 position)
        {
            return position + relativePosition;
        }

        protected bool FilterEntity(int entity)
        {
            return true;
        }

        public void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            canTrigger = timerTrigger <= elapsedTime && elapsedTime <= duration + timerTrigger;
        }

        public List<int> Collision(Vector2 position)
        {
            if (canTrigger)
            {
                return OnCollision(position);
            }

            return null;
        }

        protected abstract List<int> OnCollision(Vector2 position);

        public virtual void Gizmos(Vector3 position, Color color, float deltaTime)
        {
        }
    }
}