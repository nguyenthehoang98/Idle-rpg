using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using _KITSystem.Utils;
using UnityEngine;

namespace _KITSystem.Movement
{
#if UNITY_EDITOR
    [Serializable]
#endif
    public struct RunMovementAction : IMovementAction
    {
        [SerializeField] private float2 direction;
        [SerializeField] private float speed;
        [SerializeField] private bool useLifeTime;
        [SerializeField] private float remainingLifeTime;
        [SerializeField] private bool useDestination;
        [SerializeField] private float2 destination;
        [SerializeField] private float stopDistance;

        public int Priority => PriorityModifierIndex.DEFAULT;
        public ModifierName Name => ModifierName.Default;

        public float2 EvaluatePosition(float deltaTime)
        {
            return float2.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason { get; private set; }
        public bool OverrideOthers => false;

        public RunMovementAction(Vector2 direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
            this.useLifeTime = false;
            this.remainingLifeTime = 0;
            this.useDestination = false;
            this.destination = float2.zero;
            this.stopDistance = 0;
            this.Reason = ModifierCompleteReason.Undefined;
            this.IsFinished = false;
        }

        public RunMovementAction(Vector2 direction, float speed, float duration)
        {
            this.direction = direction;
            this.speed = speed;
            this.useLifeTime = true;
            this.remainingLifeTime = duration;
            this.useDestination = false;
            this.destination = float2.zero;
            this.stopDistance = 0;
            this.Reason = ModifierCompleteReason.Undefined;
            this.IsFinished = false;
        }

        public RunMovementAction(Vector2 direction, float speed, Vector2 destination, float stopDistance)
        {
            this.direction = direction;
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
        public float2 EvaluateVelocity(float deltaTime)
        {
            return direction * (speed * deltaTime);
        }

        public void Start(float2 startPos)
        {
        }

        public void Process(float2 position, float deltaTime)
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
                direction = MathUtils.NormalizeSafe(destination - position);
                if (math.distance(destination, position) <= stopDistance)
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
