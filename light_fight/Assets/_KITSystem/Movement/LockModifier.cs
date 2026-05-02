using System;
using UnityEngine;

namespace _KITSystem.Movement
{
    // Có thể sẽ thêm modifier Unlock Modifier.
    public struct LockModifier : IModifier
    {
        public int Priority => PriorityModifierIndex.LOCK;
        public ModifierName Name => ModifierName.Lock;

        private float duration;
        private float elapsedTime;
        private bool shouldFinish;

        public LockModifier(float duration)
        {
            this.duration = duration;
            this.elapsedTime = 0f;
            this.IsFinished = shouldFinish = false;
            this.Reason = ModifierCompleteReason.Undefined;
        }

        public void OnStart(Vector3 startPos)
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
            return Vector3.zero;
        }

        public bool IsFinished { get; private set; }
        public ModifierCompleteReason Reason { get; private set; }
        public bool OverrideOthers => true;
    }
}