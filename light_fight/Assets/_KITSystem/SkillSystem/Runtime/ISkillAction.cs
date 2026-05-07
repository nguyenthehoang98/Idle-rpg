namespace _KITSystem.SkillSystem.Runtime
{
    public interface ISkillAction
    {
        void Start();
        void Trigger(int id);
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