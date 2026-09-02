namespace _GameToolkit.SkillSystem
{
    public interface IAction
    {
        void Startup();
        void Tick(float deltaTime);
        void Interrupt();
        void Shutdown();
       
        bool IsCompleted { get; }        
        ActionCompleteReason CompleteReason { get; }
    }
}