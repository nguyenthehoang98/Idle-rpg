using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal class ParabolicTrajectoryAction : BaseTrajectoryAction
    {
        private static readonly float3 back = new float3(0, 0, -1);
        private BlendConstValue height;
        private BlendConstValue distance;

        private float duration;
        private float elapsedTime;

        public ParabolicTrajectoryAction(BlendConstValue height, BlendConstValue distance,
            float duration, float2 start, float2 goal) : base(start, goal)
        {
            this.duration = duration;
            this.height = height;
            this.distance = distance;
            this.duration = duration;
        }

        protected override float2 OnEvaluatePosition(float deltaTime)
        {
            elapsedTime += deltaTime;
            float f = Mathf.Clamp01(elapsedTime / duration);
            float h = height.Evaluate(f);
            float d = distance.Evaluate(f);
            float3 cross = math.cross(new float3(direction.x, direction.y, 0), back);
            float2 cr = new float2(cross.x, cross.y);
            return start + direction * d + cr * h;
        }
    }
}