using System;
using Unity.Mathematics;

namespace _KITSystem.Movement
{
    public struct LockMovementAction : IMovementAction
    {
        public int Priority => PriorityModifierIndex.LOCK;
        public ModifierName Name => ModifierName.Lock;

        private float duration;
        private float elapsedTime;
        private bool shouldFinish;

        public LockMovementAction(float duration)
        {
            this.duration = duration;
            this.elapsedTime = 0f;
            this.IsFinished = shouldFinish = false;
            this.Reason = ModifierCompleteReason.Undefined;
        }

        public void Start(float2 startPos)
        {

        }

        public void Process(float2 position, float deltaTime)
        {
            if (shouldFinish && !IsFinished)
            {
                IsFinished = true;
            }

            elapsedTime += deltaTime;
            if (elapsedTime > duration && !shouldFinish)
            {
                shouldFinish = true;
                Reason = ModifierCompleteReason.EndLifeCycle;
            }
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
            return float2.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason { get; private set; }
        public bool OverrideOthers => true;
    }
}
