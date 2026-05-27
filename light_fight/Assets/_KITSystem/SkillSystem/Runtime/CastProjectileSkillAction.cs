using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Entity;
using UnityEngine;

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

        protected void Damage(int entity)
        {
            int dmg = DamageOutput(entity);
            
            ref var health = ref ComponentManager<HealthData>.Get(entity);

            health.CurrentHealth -= dmg;
            
            if (health.CurrentHealth <= 0)
            {
                EntityManager.DestroyEntity(entity);
            }
            else
            {
                OnDamageEffect(entity);                
            }
        }

        protected virtual void OnDamageEffect(int entity)
        {
            EntityManager.InvokeBehaviour(entity, EntityManagerBehaviourType.BeHit);
        }

        protected virtual int DamageOutput(int entity) => 1;
    }
}