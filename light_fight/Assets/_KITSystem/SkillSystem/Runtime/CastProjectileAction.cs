using _KITSystem.SkillSystem.Config;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class CastProjectileAction : BaseAction
    {
        protected readonly BaseShapeAction[] shapes;
        
        protected internal CastProjectileAction(BaseShapeAction[] shapes, Trigger trigger, float lifeTime) : base(trigger, lifeTime)
        {
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