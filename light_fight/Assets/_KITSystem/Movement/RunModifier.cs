using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.Movement
{
    public struct RunModifier : IModifier
    {
        private Vector3 direction;
        private float speed;
        private bool useLifeTime;
        private float remainingLifeTime;

        public int Priority => 0;
        public ModifierName Name => ModifierName.Default;

        public bool IsFinished { get; private set; }
        public bool OverrideOthers => false;

        public RunModifier(Vector3 direction, float speed)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = false;
            this.remainingLifeTime = 0;
            IsFinished = false;
        }

        public RunModifier(Vector3 direction, float speed, float duration)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = true;
            this.remainingLifeTime = duration;
            IsFinished = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return direction * speed * deltaTime;
        }

        public void OnStart(Vector3 startPos)
        {
        }

        public void Tick(float deltaTime)
        {
            if (useLifeTime && !IsFinished)
            {
                remainingLifeTime -= deltaTime;
                if (remainingLifeTime <= 0)
                {
                    IsFinished = true;
                }
            }
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