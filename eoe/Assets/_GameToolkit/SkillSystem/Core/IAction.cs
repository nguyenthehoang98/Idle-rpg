namespace _KITSystem.SkillSystem.Core
{
    public interface IAction
    {
        void Start();
        void Tick(float deltaTime);
        void Interrupt();
        void Stop();
       
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