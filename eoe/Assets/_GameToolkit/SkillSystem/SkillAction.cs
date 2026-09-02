using System;
using UnityEngine;

namespace _GameToolkit.SkillSystem
{
    public abstract class SkillAction : IAction
    {
        private readonly float lifeTime;
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
            Debug.Log("Starting skill action " + GetHashCode());
        }

        protected abstract void OnTick(float deltaTime);

        public virtual void Shutdown()
        {
            Debug.Log("Stopping skill action " + GetHashCode());
            OnComplete?.Invoke();
        }

        public void Interrupt() => Complete(ActionCompleteReason.Interrupt);

        public void EndCycle() => Complete(ActionCompleteReason.EndLifeCycle);

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