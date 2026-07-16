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

        private Phase phase = Phase.Undefined;
        private Vector2 deltaPosition;
        private Vector2 outboundEndPosition;
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
            if (phase == Phase.Undefined)
            {
                phase = Phase.Outbound;
                elapsedTime = 0f;
                OnChangePhase?.Invoke(phase);
            }

            elapsedTime += deltaTime;
            
            float p = 0, f = 0, s = 0;

            switch (phase)
            {
                case Phase.Outbound:
                    
                    p = initialDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / initialDuration);
                 
                    if (elapsedTime > initialDuration)
                    {
                        if (delayDuration > 0) phase = Phase.Hang;
                        else phase = Phase.Return;
                        
                        elapsedTime = 0;
                        outboundEndPosition = deltaPosition;
                        
                        OnChangePhase?.Invoke(phase);
                    }
                    else
                    {
                        f = initialCurve.Evaluate(p);
                        
                        s = f * initialSpeed * deltaTime;
                        
                        deltaPosition += s * Direction;;
                    }

                    break;
                case Phase.Hang:
                    if (elapsedTime >= delayDuration)
                    {
                        phase = Phase.Return;
                        elapsedTime = 0;
                        OnChangePhase?.Invoke(phase);
                    }
                    break;
                case Phase.Return:
                    p = returnDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / returnDuration);
                   
                    f = returnCurve.Evaluate(p);

                    deltaPosition = Vector2.Lerp(outboundEndPosition, Vector2.zero, f);

                    if (elapsedTime >= returnDuration)
                    {
                        deltaPosition = Vector2.zero;
                        
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
           Undefined, Outbound, Hang, Return, Complete
        }
    }
}