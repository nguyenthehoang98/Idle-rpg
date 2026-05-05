using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastRangeProjectileAction : CastProjectileAction
    {
        private BaseTrajectoryAction trajectory;
        
        internal CastRangeProjectileAction(BaseTrajectoryAction trajectory,
            BaseShapeAction[] shapes, TriggerConfig triggerConfig, float lifeTime) : base(shapes, triggerConfig, lifeTime)
        {
            this.trajectory = trajectory;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            Vector3 position = trajectory.EvaluatePosition(deltaTime);
            for (int i = 0; i < shapes.Length; i++)
            {
                var shape = shapes[i];
                bool hit = shape.Hit(position, out List<int> list);
#if UNITY_EDITOR
                shape.Gizmos(position, hit ? Color.red : Color.green, deltaTime);
#endif
                if (hit)
                {
                    foreach (var hitId in list) Damage(hitId);
                }
            }
        }
    }
}