using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    public abstract class BaseSkillAction : ISkillAction
    {
        protected readonly SPU spu;
        private float lifeTime;
        private TriggerConfig.TriggerType type;
        private int eventId;
        private bool isMultiplierTrigger;
        private float timer;

        private bool hasTriggered;
        private float elapsedTime;

        protected BaseSkillAction(SPU spu, TriggerConfig triggerConfig, float lifeTime)
        {
            this.spu = spu;
            this.lifeTime = lifeTime;
            type = triggerConfig.type;
            eventId = triggerConfig.eventId;
            timer = triggerConfig.timer;
            isMultiplierTrigger = triggerConfig.isMultiplierTrigger;
            Reason = ActionCompleteReason.Undefined;
        }

        public void Start()
        {
            OnStart();
        }

        public void Trigger(int id)
        {
            if (IsFinished) return;
            
            if (type == TriggerConfig.TriggerType.Event 
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
            
            if (type == TriggerConfig.TriggerType.Timeline 
                && !hasTriggered 
                && elapsedTime >= timer)
            {
                hasTriggered = true;
                Execute();
            }
            
            if(hasTriggered) OnUpdate(deltaTime);

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