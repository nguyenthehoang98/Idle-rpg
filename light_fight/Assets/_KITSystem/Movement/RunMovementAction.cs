using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.Movement
{
#if UNITY_EDITOR
    [Serializable]
#endif
    public struct RunMovementAction : IMovementAction
    {
        [SerializeField] private Vector3 direction;
        [SerializeField] private float speed;
        [SerializeField] private bool useLifeTime;
        [SerializeField] private float remainingLifeTime;
        [SerializeField] private bool useDestination;
        [SerializeField] private Vector3 destination;
        [SerializeField] private float stopDistance;

        public int Priority => PriorityModifierIndex.DEFAULT;
        public ModifierName Name => ModifierName.Default;

        public Vector3 EvaluatePosition(float deltaTime)
        {
            return Vector3.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason { get; private set; }
        public bool OverrideOthers => false;

        public RunMovementAction(Vector3 direction, float speed)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = false;
            this.remainingLifeTime = 0;
            this.useDestination = false;
            this.destination = Vector3.zero;
            this.stopDistance = 0;
            this.Reason = ModifierCompleteReason.Undefined;
            this.IsFinished = false;
        }

        public RunMovementAction(Vector3 direction, float speed, float duration)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = true;
            this.remainingLifeTime = duration;
            this.useDestination = false;
            this.destination = Vector3.zero;
            this.stopDistance = 0;
            this.Reason = ModifierCompleteReason.Undefined;
            this.IsFinished = false;
        }

        public RunMovementAction(Vector3 direction, float speed, Vector3 destination, float stopDistance)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.useLifeTime = false;
            this.remainingLifeTime = 0;
            this.useDestination = true;
            this.destination = destination;
            this.stopDistance = stopDistance;
            this.Reason = ModifierCompleteReason.Undefined;
            this.IsFinished = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return direction * (speed * deltaTime);
        }

        public void Start(Vector3 startPos)
        {
        }

        public void Process(Vector3 position, float deltaTime)
        {
            if (useLifeTime && !IsFinished)
            {
                remainingLifeTime -= deltaTime;
                if (remainingLifeTime <= 0)
                {
                    Reason = ModifierCompleteReason.EndLifeCycle;
                    IsFinished = true;
                }
            }
            
            if (useDestination && !IsFinished)
            {
                direction = (destination - position).normalized;
                if (Vector3.Distance(destination, position) <= stopDistance)
                {
                    Reason = ModifierCompleteReason.EndLifeCycle;
                    IsFinished = true;
                }
            }
        }

        public void Stop()
        {
        }

        public void Interrupt()
        {
            Reason = ModifierCompleteReason.Interrupt;
            IsFinished = true;
        }
    }
}