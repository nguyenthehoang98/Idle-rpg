using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class CastProjectileSkillAction : BaseSkillAction
    {
        protected readonly BaseShapeAction[] shapes;
        protected readonly SPU spu;
        
        protected internal CastProjectileSkillAction(BaseShapeAction[] shapes,
            SPU spu, TriggerConfig triggerConfig, float lifeTime) : base(triggerConfig, lifeTime)
        {
            this.spu = spu;
            this.shapes = shapes;
        }

        protected override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < shapes.Length; i++)
            {
                shapes[i].Tick(deltaTime);
            }
        }

        protected void Damage(int target) => OnDamage(target);

        protected virtual void OnDamage(int target)
        {
        }
    }
}