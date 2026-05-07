using UnityEngine;

namespace _KITSystem.Movement
{
    public struct TeleportMovementAction : IMovementAction
    {
        private Vector3 destination;

        public TeleportMovementAction(Vector3 destination)
        {
            this.destination = destination;
            IsFinished = false;
        }

        public int Priority => PriorityModifierIndex.TELEPORT;
        public ModifierName Name => ModifierName.Teleport;
        public void Start(Vector3 startPos)
        {
        }

        public void Process(Vector3 position, float deltaTime)
        {
        }

        public void Stop()
        {
        }

        public void Interrupt()
        {
        }

        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return Vector3.zero;
        }

        public Vector3 EvaluatePosition(float deltaTime)
        {
            if (!IsFinished)
            {
                IsFinished = true;
                return destination;
            }

            return Vector3.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason => ModifierCompleteReason.EndLifeCycle;
        public bool OverrideOthers => false;
    }
}