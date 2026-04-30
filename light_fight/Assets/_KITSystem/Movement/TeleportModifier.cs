using UnityEngine;

namespace _KITSystem.Movement
{
    public struct TeleportModifier : IModifier
    {
        private Vector3 destination;

        public TeleportModifier(Vector3 destination)
        {
            this.destination = destination;
            IsFinished = false;
        }

        public int Priority => 5;
        public ModifierName Name => ModifierName.Teleport;
        public void OnStart(Vector3 startPos)
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void ProcessPosition(Vector3 position)
        {
        }

        public void OnEnd()
        {
        }

        public void OnInterrupt()
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
        public bool OverrideOthers => false;
    }
}