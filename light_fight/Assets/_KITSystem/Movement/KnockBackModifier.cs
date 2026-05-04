using System;
using UnityEngine;

namespace _KITSystem.Movement
{
    public struct KnockBackModifier : IModifier
    {
        public int Priority => PriorityModifierIndex.KNOCK_BACK;
        public ModifierName Name => ModifierName.KnockBack;

        private Vector3 direction;
        private float duration;
        private float distance;
        private float elapsedTime;
        private float traveled;
        private bool shouldFinish;
        private AnimationCurve curve;
        private Action onComplete;

        public KnockBackModifier(Vector3 direction, float duration, float distance, AnimationCurve curve, Action onComplete)
        {
            this.direction = direction.normalized;
            this.duration = duration;
            this.distance = distance;
            this.curve = curve;
            this.onComplete = onComplete;
            this.IsFinished = shouldFinish = false;
            this.elapsedTime = traveled = 0;
            this.Reason = ModifierCompleteReason.Undefined;
        }

        public KnockBackModifier(Vector3 direction, float duration, float distance, Action onComplete)
        {
            this.direction = direction.normalized;
            this.duration = duration;
            this.distance = distance;
            this.onComplete = onComplete;
            this.curve = AnimationCurve.Linear(0, 0, 1, 1);        
            this.IsFinished = shouldFinish = false;
            this.elapsedTime = traveled = 0;
            this.Reason = ModifierCompleteReason.Undefined;
        }

        public void Start(Vector3 startPos)
        {
        }

        public void Process(Vector3 position, float deltaTime)
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
            onComplete?.Invoke();
        }

        public void Interrupt()
        {
            IsFinished = true;
        }

        public Vector3 EvaluateVelocity(float deltaTime)
        {
            return Vector3.zero;
        }

        public Vector3 EvaluatePosition(float deltaTime)
        {
            float f = elapsedTime / duration;
            float p = curve.Evaluate(f);
            float d = p * distance;
            float s = d - traveled;
            traveled = d;
            return s * direction;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason { get; private set; }
        public bool OverrideOthers => true;
    }
}