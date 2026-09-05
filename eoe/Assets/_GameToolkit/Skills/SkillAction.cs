using System;
using UnityEngine;

namespace _GameToolkit.Skills
{
    public abstract class SkillAction : ISkillAction
    {
        private float lifeTime;
        private float elapsedTime;
        
        public event Action OnComplete;

        public bool IsCompleted { get; protected set; }

        public ActionCompleteReason CompleteReason { get; protected set; }

        protected SkillAction(float lifeTime)
        {
            this.lifeTime = lifeTime;
            
            this.CompleteReason = ActionCompleteReason.Undefined;
        }

        public void Tick(float deltaTime)
        {
            if (!IsCompleted)
            {
                elapsedTime += deltaTime;

                OnTick(deltaTime);

                if (elapsedTime >= lifeTime) EndCycle();
            }
        }

        public virtual void Startup()
        {
        }

        protected abstract void OnTick(float deltaTime);

        public virtual void Shutdown()
        {
            OnComplete?.Invoke();
        }

        public void Interrupt() => Complete(ActionCompleteReason.Interrupt);

        public void EndCycle() => Complete(ActionCompleteReason.EndLifeCycle);

        public void Refresh(float duration)
        {
            if (IsCompleted) return;
            lifeTime = Mathf.Max(0.01f, duration);
            elapsedTime = 0f;
        }

        private void Complete(ActionCompleteReason reason)
        {
            if (!IsCompleted)
            {
                CompleteReason = reason;

                IsCompleted = true;
            }
        }
    }
}