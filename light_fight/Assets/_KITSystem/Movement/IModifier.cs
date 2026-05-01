using UnityEngine;

namespace _KITSystem.Movement
{
    public interface IModifier
    {
        int Priority { get; }
        ModifierName Name { get; }
        
        // ~start/update/stop
        void OnStart(Vector3 startPos);
        void Tick(float deltaTime);
        void ProcessPosition(Vector3 position);
        void OnEnd(); //~end lifecycle
        void OnInterrupt(); //~force end
        Vector3 EvaluateVelocity(float deltaTime);
        Vector3 EvaluatePosition(float deltaTime);
        
        // ~end cycle?
        bool IsFinished { get; }
        ModifierCompleteReason Reason { get; }
        /// <summary>
        /// True: sẽ xóa các modifier có priority nhỏ hơn nó
        /// </summary>
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
    }
}