using System;
using _GameToolkit.SkillSystem.Core;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Imp
{
    public class BoomerangTrajectory : BaseTrajectory
    {
        private readonly AnimationCurve initialCurve;
        private readonly AnimationCurve returnCurve;
        private readonly float initialDuration;
        private readonly float delayDuration;
        private readonly float returnDuration;

        public event Action<Phase> OnChangePhase;

        private Phase phase = Phase.Undefined;
        private Vector2 deltaPosition;
        private Vector2 outboundEndPosition;
        private float elapsedTime;
        private bool isCompleted;
        
        public BoomerangTrajectory(AnimationCurve initialCurve, AnimationCurve returnCurve, float initialDuration,
            float delayDuration, float returnDuration,
            Vector2 start, Vector2 goal) : base(start, goal)
        {
            this.initialCurve = initialCurve;
            
            this.returnCurve = returnCurve;
            
            this.delayDuration = delayDuration;
            
            this.initialDuration = initialDuration;
            
            this.returnDuration = returnDuration;
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            if (phase == Phase.Undefined)
            {
                phase = Phase.Outbound;
                elapsedTime = 0f;
                OnChangePhase?.Invoke(phase);
            }

            elapsedTime += deltaTime;

            float p, f;
            float d;

            switch (phase)
            {
                case Phase.Outbound:
                    d = initialDuration - deltaTime;
                    p = initialDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / d);

                    if (elapsedTime >= d)
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

                        deltaPosition = Vector2.Lerp(Start, Goal, f);
                    }

                    break;
                case Phase.Hang:
                    d = delayDuration - deltaTime;
                    if (elapsedTime >= d)
                    {
                        phase = Phase.Return;
                        elapsedTime = 0;
                        OnChangePhase?.Invoke(phase);
                    }

                    break;
                case Phase.Return:
                    d = returnDuration - deltaTime;
                    
                    p = returnDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / d);

                    if (elapsedTime >= d)
                    {
                        deltaPosition = Vector2.zero;

                        elapsedTime = 0;

                        phase = Phase.Complete;
                    }
                    else
                    {
                        f = returnCurve.Evaluate(p);

                        deltaPosition = Vector2.Lerp(outboundEndPosition, Start, f);
                    }

                    break;
                case Phase.Complete:
                    if (!isCompleted)
                    {
                        isCompleted = true;
                        OnChangePhase?.Invoke(phase);
                    }

                    break;
            }

            return deltaPosition;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            if (phase == Phase.Outbound) return Direction;
         
            if (phase == Phase.Return) return -Direction;
            
            return Vector2.zero;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (!isCompleted)
            {
                isCompleted = true;
                OnChangePhase?.Invoke(phase);
            }
        }

        public enum Phase
        {
           Undefined, Outbound, Hang, Return, Complete
        }
    }
}