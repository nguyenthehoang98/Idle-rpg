using UnityEngine;

namespace _KITSystem.Movement
{
    public struct RunModifier : IModifier
    {
        private Vector3 direction;
        private float speed;

        public int Priority => 0;
        public ModifierName Name => ModifierName.Default;

        public bool IsFinished { get; private set; }
        public bool OverrideOthers => false;

        public RunModifier(Vector3 direction, float speed)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            IsFinished = false;
        }

        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return direction * speed * deltaTime;
        }

        public void OnStart(Vector3 startPos)
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void OnEnd()
        {
        }

        public void OnInterrupt()
        {
            IsFinished = true;
        }
    }
}