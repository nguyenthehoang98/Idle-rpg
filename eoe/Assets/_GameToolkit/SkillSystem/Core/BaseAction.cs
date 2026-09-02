namespace _KITSystem.SkillSystem.Core
{
    public abstract class BaseAction : IAction
    {
        protected readonly float LifeTime;

        private float elapsedTime;

        protected BaseAction(float lifeTime)
        {
            LifeTime = lifeTime;
            Reason = ActionCompleteReason.Undefined;
        }

        public void Start()
        {
            OnStart();
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            elapsedTime += deltaTime;

            OnUpdate(deltaTime);

            if (elapsedTime >= LifeTime) EndLifeCycle();
        }

        public void Interrupt()
        {
            if (!IsFinished)
            {
                Reason = ActionCompleteReason.Interrupt;
                IsFinished = true;
            }
        }

        public void EndLifeCycle()
        {
            if (!IsFinished)
            {
                Reason = ActionCompleteReason.EndLifeCycle;
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

        protected virtual void OnStop()
        {
        }

        public bool IsFinished { get; private set; }

        public ActionCompleteReason Reason { get; private set; }
    }
}