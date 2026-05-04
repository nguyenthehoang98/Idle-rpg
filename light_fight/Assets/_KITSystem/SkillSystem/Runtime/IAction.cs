namespace _KITSystem.SkillSystem.Runtime
{
    public interface IAction
    {
        void OnStart();
        void Tick(float deltaTime);
        void OnInterrupt();
        void OnEnd();
       
        bool IsFinished { get; }        
        ActionCompleteReason Reason { get; }
    }

    public enum ActionCompleteReason
    {
        Undefined,
        EndLifeCycle,
        Interrupt,
    }
}