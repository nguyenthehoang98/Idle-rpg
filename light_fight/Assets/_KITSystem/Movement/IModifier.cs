using UnityEngine;

namespace _KITSystem.Movement
{
    public interface IModifier
    {
        int Priority { get; }
        ModifierName Name { get; }
        
        // ~start/update/stop
        void OnStart(Vector3 startPos);
        Vector3 EvaluatePosition(float elapsedTime);
        void OnEnd(); //~end lifecycle
        void OnInterrupt(); //~force end
        
        // ~end cycle?
        bool IsFinished(float elapsedTime);
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