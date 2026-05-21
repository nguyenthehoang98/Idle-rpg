using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Entity;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract class CastProjectileSkillAction : BaseSkillAction
    {
        protected readonly BaseShapeAction[] shapes;
        
        protected internal CastProjectileSkillAction(BaseShapeAction[] shapes,
            SPU spu, TriggerConfig triggerConfig, float lifeTime) : base(spu, triggerConfig, lifeTime)
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

        protected void Damage(int entity) => OnDamage(entity);

        protected virtual void OnDamage(int entity)
        {
            ref var health = ref ComponentManager<HealthData>.Get(entity);
            health.CurrentHealth -= 10;
            if (health.CurrentHealth <= 0)
            {
                EntityManager.DestroyEntity(entity);
            }
        }
    }
}