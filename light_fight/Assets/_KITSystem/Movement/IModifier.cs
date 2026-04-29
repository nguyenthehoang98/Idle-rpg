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
        void OnEnd(); //~end lifecycle
        void OnInterrupt(); //~force end
        Vector3 EvaluateVelocity(float deltaTime);
        
        // ~end cycle?
        bool IsFinished { get; }
        /// <summary>
        /// True: sẽ xóa các modifier có priority nhỏ hơn nó
        /// </summary>
        bool OverrideOthers { get; } 
    }

    public enum ModifierName
    {
        Default, 
        Testing,
    }
}