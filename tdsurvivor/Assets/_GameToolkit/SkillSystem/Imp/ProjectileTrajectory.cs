using _GameToolkit.SkillSystem.Core;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Imp
{
    public class ProjectileTrajectory : BaseTrajectory
    {
        private readonly AnimationCurve curve;
        private readonly float speed;
        private readonly float duration;

        private Vector2 deltaPosition;
        private float elapsedTime;
        private bool isCompleted;

        public ProjectileTrajectory(AnimationCurve curve, float speed, float duration, Vector2 start, Vector2 goal) :
            base(start, goal)
        {
            this.speed = speed;
            this.curve = curve;
            this.duration = duration;
        }

        public override Vector2 EvaluatePosition(float deltaTime)
        {
            if (!isCompleted)
            {
                elapsedTime += deltaTime;
                
                float p = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
                
                float f = curve.Evaluate(p);
                
                // Tổng quãng đường = speed * duration, curve chỉ "làm duyên" tiến trình
                // (xác định, không phụ thuộc frame-rate như tích phân += trước đây)
                deltaPosition = Direction * (f * speed * duration);
                
                if (p >= 1f) isCompleted = true;
            }

            return deltaPosition + Start;
        }

        public override Vector2 EvaluateDirection(float deltaTime)
        {
            return Direction;
        }
    }
}