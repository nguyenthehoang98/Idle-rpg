using Unity.Mathematics;

namespace _KITSystem.Movement
{
    public interface IMovementAction
    {
        int Priority { get; }
        ModifierName Name { get; }

        void Start(float2 startPos);
        void Process(float2 position, float deltaTime);
        void Stop();
        void Interrupt();
        float2 EvaluateVelocity(float deltaTime);
        float2 EvaluatePosition(float deltaTime);

        bool IsFinished { get; }
        ModifierCompleteReason Reason { get; }
        bool OverrideOthers { get; }
    }

    public enum ModifierCompleteReason
    {
        Undefined,
        EndLifeCycle,
        Interrupt,
    }

    public enum ModifierName
    {
        Default,
        Testing,
        Teleport,
        KnockBack,
        Lock,
    }
}