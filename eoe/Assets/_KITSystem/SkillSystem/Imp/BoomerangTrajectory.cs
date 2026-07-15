using System;
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
        private readonly float returnDuration;

        public event Action<Phase> OnChangePhase;

        private Phase phase = Phase.Init;
        private Vector2 deltaPosition;
        private Vector2 savedDeltaPosition;
        private float elapsedTime;

        public BoomerangTrajectory(AnimationCurve initialCurve, AnimationCurve returnCurve,
            float initialSpeed, float initialDuration,
            float delayDuration, float returnDuration,
            Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.initialCurve = initialCurve;
            this.returnCurve = returnCurve;
            this.delayDuration = delayDuration;
            this.initialSpeed = initialSpeed;
            this.initialDuration = initialDuration;
            this.returnDuration = returnDuration;
        }

        protected override Vector2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float p, f, s;
            switch (phase)
            {
                case Phase.Init:
                    p = Mathf.Clamp01(elapsedTime / initialDuration);
                    f = initialCurve.Evaluate(p);
                    s = f * initialSpeed * deltaTime;
                    deltaPosition += s * Direction;
              
                    if (p >= 1)
                    {
                        if (delayDuration > 0) phase = Phase.Wait;
                        else phase = Phase.Return;
                        elapsedTime = 0;
                        savedDeltaPosition = deltaPosition;
                        OnChangePhase?.Invoke(phase);
                    }
                    break;
                case Phase.Wait:
                    if (elapsedTime >= delayDuration)
                    {
                        phase = Phase.Return;
                        elapsedTime = 0;
                        OnChangePhase?.Invoke(phase);
                    }
                    break;
                case Phase.Return:
                    p = elapsedTime / returnDuration;
                    f = returnCurve.Evaluate(p);
                    deltaPosition = Vector2.Lerp(savedDeltaPosition, Vector2.zero, f);
               
                    if (p >= 1)
                    {
                        elapsedTime = 0;
                        phase = Phase.Complete;
                        OnChangePhase?.Invoke(phase);
                    }
                    break;
                case Phase.Complete:
                    break;
            }

            return deltaPosition + Start;
        }

        public enum Phase
        {
            Init, Wait, Return, Complete
        }
    }
}