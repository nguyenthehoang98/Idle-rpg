using System.Collections.Generic;
using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastRangeProjectileSkillAction : CastProjectileSkillAction
    {
        private BaseTrajectoryAction trajectory;
        
        internal CastRangeProjectileSkillAction(BaseTrajectoryAction trajectory, BaseShapeAction[] shapes, 
            SPU spu, TriggerConfig triggerConfig, float lifeTime) : base(shapes, spu, triggerConfig, lifeTime)
        {
            this.trajectory = trajectory;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            float2 position = trajectory.EvaluatePosition(deltaTime);
            for (int i = 0; i < shapes.Length; i++)
            {
                var shape = shapes[i];
                bool hit = shape.Hit(position, out List<int> list);
#if UNITY_EDITOR
                shape.Gizmos(new Vector3(position.x, position.y), hit ? Color.red : Color.green, deltaTime);
#endif
                if (hit)
                {
                    foreach (var hitId in list) Damage(hitId);
                }
            }
        }
    }
}