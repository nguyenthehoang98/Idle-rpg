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
        private bool useDestination;
        private Vector3 destination;
        private float stopDistance;

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
            this.useDestination = false;
            this.destination = Vector3.zero;
            this.stopDistance = 0;
            IsFinished = false;
        }

        public RunModifier(Vector3 direction, float speed, float duration)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = true;
            this.remainingLifeTime = duration;
            this.useDestination = false;
            this.destination = Vector3.zero;
            this.stopDistance = 0;
            IsFinished = false;
        }

        public RunModifier(Vector3 direction, float speed, Vector3 destination, float stopDistance)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = false;
            this.remainingLifeTime = 0;
            this.useDestination = true;
            this.destination = destination;
            this.stopDistance = stopDistance;
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

        public void ProcessPosition(Vector3 position)
        {
            if (useDestination && !IsFinished)
            {
                direction = (destination - position).normalized;
                if (Vector3.Distance(destination, position) <= stopDistance)
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