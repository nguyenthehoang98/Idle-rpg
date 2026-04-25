using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Data;
using _KIT.Pool;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class ProjectileAuthoring : MonoBehaviour, IAuthoring
    {
        private Queue<Action> queue = new Queue<Action>();
        
        public void Initialize(Entity entity)
        {
        }

        public void Hit(int skillId, Vector3 position)
        {
            Action action = () =>
            {
                int newSkillId = 2005;
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery  query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(LocalTransform), typeof(HealthData));
                ECSFactory.PrepareProjectileBuild(query, newSkillId, position, tuple =>
                {
                    ECSFactory.BuildProjectile(manager, Entity.Null, tuple.target,
                        position, tuple.targetPosition, tuple.skill, tuple.skillData, 1
                    );
                });
            };
            queue.Enqueue(action);
        }

        private void FixedUpdate()
        {
            while (queue.Count > 0)
            {
                queue.Dequeue().Invoke();
            }
        }

        public void Destroy()
        {
            Action action = () => { KitPool.Destroy(gameObject); };
            queue.Enqueue(action);
        }
    }
}
