using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastMeleeProjectileSkillAction : CastProjectileSkillAction
    {
        private bool[] triggered;
        private float2 start;
        private float2 goal;

        internal CastMeleeProjectileSkillAction(float2 start, float2 goal, BaseShapeAction[] shapes,
            SPU spu, TriggerConfig triggerConfig, float lifeTime) 
            : base(shapes, spu, triggerConfig, lifeTime)
        {
            this.goal = goal;
            this.start = start;
            triggered = new bool[shapes.Length];
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            for (int i = 0; i < triggered.Length; i++)
            {
                if (triggered[i]) continue;
                
                var shape = shapes[i];
                if (shape.CanTrigger)
                {
                    shape.Hit(start, results =>
                    {
                        bool hit = results.Count > 0;
#if UNITY_EDITOR
                        shape.Gizmos(new Vector3(start.x, start.y), hit ? Color.red : Color.green, deltaTime);
#endif
                        if (hit)
                        {
                            for (int i1 = 0; i1 < results.Count; i1++)
                            {
                                int hitId = results[i1];
                                Damage(hitId);
                            }
                        }
                    });
                    
                    triggered[i] = true;
                }
            }
        }
    }
}