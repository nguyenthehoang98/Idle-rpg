namespace _KITSystem.SkillSystem.Model
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
}