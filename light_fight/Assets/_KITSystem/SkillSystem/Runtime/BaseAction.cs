using _KITSystem.SkillSystem.Config.Model;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    public abstract class BaseAction : IAction
    {
        private float lifeTime;
        private Trigger.TriggerType type;
        private int eventId;
        private bool isMultiplierTrigger;
        private float timer;

        private bool hasTriggered;
        private float elapsedTime;

        protected BaseAction(Trigger.TriggerType type, int eventId, float timer, bool isMultiplierTrigger,
            float lifeTime)
        {
            this.lifeTime = lifeTime;
            this.type = type;
            this.eventId = eventId;
            this.timer = timer;
            this.isMultiplierTrigger = isMultiplierTrigger;
            this.Reason = ActionCompleteReason.Undefined;
        }

        public void Start()
        {
            OnStart();
        }

        public void Trigger(int id)
        {
            if (IsFinished) return;

            if (type == Config.Model.Trigger.TriggerType.Event 
                && eventId == id 
                && (!hasTriggered || isMultiplierTrigger))
            {
                hasTriggered = true;
                Execute();
            }
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            elapsedTime += deltaTime;
            
            if (type == Config.Model.Trigger.TriggerType.Timeline 
                && !hasTriggered 
                && elapsedTime >= timer)
            {
                hasTriggered = true;
                Execute();
            }
            
            OnUpdate(deltaTime);

            if (elapsedTime >= lifeTime)
            {
                Reason = ActionCompleteReason.EndLifeCycle;
                IsFinished = true;
            }
        }

        public void Interrupt()
        {
            if (!IsFinished)
            {
                Reason = ActionCompleteReason.Interrupt;
                IsFinished = true;
            }
        }

        public void Stop()
        {
            OnStop();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }

        protected virtual void Execute()
        {
        }

        protected virtual void OnStop()
        {
        }

        public bool IsFinished { get; private set; }
        public ActionCompleteReason Reason { get; private set; }
    }
}