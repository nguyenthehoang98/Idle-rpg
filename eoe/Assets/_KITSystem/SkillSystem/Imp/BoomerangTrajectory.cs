using _KITSystem.SkillSystem.Core;
using UnityEngine;

namespace _KITSystem.SkillSystem.Imp
{
    public class BoomerangTrajectory : BaseTrajectory
    {
        private readonly AnimationCurve initialCurve;
        private readonly AnimationCurve returnCurve;
        private readonly float initialSpeed;
        private readonly float initialDuration;
        private readonly float delayDuration;
        private readonly float returnSpeed;
        private readonly float returnDuration;

        private bool isInitialized;
        private bool isWaitingPhase;
        private bool isReturningPhase;

        private Vector2 deltaPosition;
        private float elapsedTime;

        public BoomerangTrajectory(AnimationCurve initialCurve, AnimationCurve returnCurve,
            float initialSpeed, float initialDuration,
            float delayDuration,
            float returnSpeed, float returnDuration,
            Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.initialCurve = initialCurve;
            this.returnCurve = returnCurve;
            this.delayDuration = delayDuration;
            this.initialSpeed = initialSpeed;
            this.initialDuration = initialDuration;
            this.returnDuration = returnDuration;
            this.returnSpeed = returnSpeed;
            this.isInitialized = true;
        }

        protected override Vector2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (isReturningPhase)
            {
                float p = elapsedTime / returnDuration;
                float f = returnCurve.Evaluate(p);
                float s = f * returnSpeed * deltaTime;
                deltaPosition -= s * Direction;
               
                if (p >= 1)
                {
                    isReturningPhase = false;
                }
            }
            else if (isWaitingPhase)
            {
                if (elapsedTime >= delayDuration)
                {
                    elapsedTime = 0;
                    isWaitingPhase = false;
                    isReturningPhase = true;
                }
            }
            else if (isInitialized)
            {
                float p = Mathf.Clamp01(elapsedTime / initialDuration);
                float f = initialCurve.Evaluate(p);
                float s = f * initialSpeed * deltaTime;
                deltaPosition += s * Direction;
              
                if (p >= 1)
                {
                    isInitialized = false;
                    isWaitingPhase = true;
                }
            }

            return deltaPosition + Start;
        }
    }
}