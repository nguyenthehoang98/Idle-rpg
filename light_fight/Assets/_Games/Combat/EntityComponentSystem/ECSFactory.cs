using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.EntityComponentSystem.View;
using _KIT.Config;
using _KIT.Pool;
using _KIT.Resource;
using ProjectDawn.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem
{
    public static class ECSFactory
    {
        private static HashSet<string> monstersPath = new HashSet<string>();
        
        public static Entity BuildPlayer()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = manager.CreateEntity();
#if UNITY_EDITOR
            manager.SetName(entity, "Player");
#endif
            return entity;
        }

        public static async void BuildMonster(int monsterID, float bonusRange,
            float3 position, float3 destination)
        {
            KitConfigManager.Get<MonsterConfig>().Find(monsterID, out var monsterData);
            GameObject go = await KitLoaded.LoadAsync<GameObject>(monsterData.MonsterObjectId, true);
            if (monstersPath.Add(monsterData.MonsterObjectId))
            {
                KitPool.RegisterPool(go, true);
            }
            EntityView view = KitPool.Instantiate(go.GetComponent<EntityView>());
            view.transform.position = position;
            
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = view.GetOrCreateEntity();
            manager.AddComponentData(entity, new MonsterTag());
            manager.AddComponentData(entity, new LocalTransform
            {
                Position = position, Rotation = quaternion.identity, Scale = 1
            });
            manager.AddComponentData(entity, new Agent
            {
                Layers = monsterData.IsRanged ? NavigationLayers.Layer1 : NavigationLayers.Default
            });
            manager.AddComponentData(entity, new AgentBody
            {
                Destination = destination
            });
            manager.AddComponentData(entity, new AgentCollider
            {
                Layers = monsterData.IsRanged ? NavigationLayers.Layer1 : NavigationLayers.Default
            });
            manager.AddComponentData(entity, new AgentLocomotion
            {
                Speed = monsterData.MoveSpeed, Acceleration = monsterData.MoveSpeed, AngularSpeed = 0,
                StoppingDistance = bonusRange + monsterData.StopMoveDistance,
                AutoBreaking = true,
            });
            manager.AddComponentData(entity, new AgentSeparation
            {
                Radius = monsterData.Radius, Weight = 1,
                Layers = monsterData.IsRanged ? NavigationLayers.Layer1 : NavigationLayers.Default
            });
            manager.AddComponentData(entity, new AgentShape
            {
                Radius = monsterData.Radius, Type = ShapeType.Circle
            });
            manager.AddComponentData(entity, new AgentSonarAvoid
            {
                Radius = monsterData.Radius * 2,
                Mode = SonarAvoidMode.IgnoreBehindAgents,
                MaxAngle = math.radians(360),
                Angle = math.radians(180),
                BlockedStop = true,
                Layers = monsterData.IsRanged ? NavigationLayers.Default : NavigationLayers.Layer1
            });
            manager.AddComponentData(entity,
                new DefaultStatData(monsterData.MoveSpeed, monsterData.Attack, monsterData.Defense, monsterData.Health)
            );
            manager.AddComponentData(entity, new HealthData
            {
                Health = monsterData.Health,
                MaxHealth = monsterData.Health,
            });
            manager.AddComponentData(entity,
                new MonsterSkillData(monsterID, monsterData.SkillId, monsterData.SkillLevel,
                    monsterData.SkillCooldown, bonusRange + monsterData.AttackDistance, 1)
            );
            if (monsterData.IsRanged)
                manager.AddComponentData(entity, new MonsterRangedTag());
            else
                manager.AddComponentData(entity, new MonsterMeleeTag());
            manager.AddComponentObject(entity, view.transform);
#if UNITY_EDITOR
            manager.SetName(entity, view.name);
#endif
            manager.SetEnabled(entity, true);
            view.gameObject.SendMessage("Initialize", entity);
        }

        public static void UnloadAll()
        {
            foreach (var path in monstersPath)
            {
                KitLoaded.UnCache(path);
            }
            
            monstersPath.Clear();
        }
    }
}