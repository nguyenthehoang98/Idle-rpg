using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.Movement
{
    public struct TeleportMovementAction : IMovementAction
    {
        private float2 destination;

        public TeleportMovementAction(Vector3 destination)
        {
            this.destination = new float2(destination.x, destination.z);
            IsFinished = false;
        }

        public int Priority => PriorityModifierIndex.TELEPORT;
        public ModifierName Name => ModifierName.Teleport;
        public void Start(float2 startPos)
        {
        }

        public void Process(float2 position, float deltaTime)
        {
        }

        public void Stop()
        {
        }

        public void Interrupt()
        {
        }

        public float2 EvaluateVelocity(float deltaTime)
        {
            return float2.zero;
        }

        public float2 EvaluatePosition(float deltaTime)
        {
            if (!IsFinished)
            {
                IsFinished = true;
                return destination;
            }

            return float2.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason => ModifierCompleteReason.EndLifeCycle;
        public bool OverrideOthers => false;
    }
}
