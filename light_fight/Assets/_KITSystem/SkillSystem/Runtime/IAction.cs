namespace _KITSystem.SkillSystem.Runtime
{
    public interface IAction
    {
        void Tick(float deltaTime);
       
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