using System;

namespace _Toolkit.SkillSystem.Core
{
    public abstract class BaseSkillAction
    {
        private float lifeTime;
        private float elapsedTime;
        
        public event Action OnComplete;

        public bool IsCompleted { get; protected set; }

        public SkillActionCompleteReason CompleteReason { get; protected set; }

        protected BaseSkillAction(float lifeTime)
        {
            this.lifeTime = lifeTime;
            
            this.CompleteReason = SkillActionCompleteReason.Undefined;
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

        public virtual void Startup() { }

        protected abstract void OnTick(float deltaTime);

        public virtual void Shutdown()
        {
            OnComplete?.Invoke();
        }

        public void Interrupt() => Complete(SkillActionCompleteReason.Interrupt);

        public void EndCycle() => Complete(SkillActionCompleteReason.EndLifeCycle);

        private void Complete(SkillActionCompleteReason reason)
        {
            if (!IsCompleted)
            {
                CompleteReason = reason;

                IsCompleted = true;
            }
        }
    }
}