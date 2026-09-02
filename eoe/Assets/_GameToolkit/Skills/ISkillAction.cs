namespace _GameToolkit.Skills
{
    public interface ISkillAction
    {
        void Startup();
        void Tick(float deltaTime);
        void Interrupt();
        void Shutdown();
       
        bool IsCompleted { get; }        
        ActionCompleteReason CompleteReason { get; }
    }
}