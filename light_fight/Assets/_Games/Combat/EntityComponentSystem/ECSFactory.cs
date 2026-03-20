using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.EntityComponentSystem.View;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem.Config;
using _Games.Combat.SkillSystem.Model;
using _KIT.Config;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Cysharp.Threading.Tasks;
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
            manager.AddComponentData(entity, new PlayerTag());
            manager.AddComponentData(entity, new HealthData { Health = 100000, MaxHealth = 100000 });
#if UNITY_EDITOR
            manager.SetName(entity, "Player");
#endif
            return entity;
        }

        public static async void BuildProjectile(EntityManager manager, Entity source,
            float3 startPosition, float3 endPosition,
            Skill skill, SkillStatData statData)
        {
            if (skill.behaviors.Length == 0)
            {
                BuildProjectileInternal(manager, source, startPosition, endPosition, skill, statData);
            }
            else
            {
                bool shouldDefaultBuildProjectile = true;
                foreach (BaseBehaviorSO behavior in skill.behaviors)
                {
                    switch (behavior.Type)
                    {
                        case BehaviourType.Spread:
                        case BehaviourType.DropStrike:
                            shouldDefaultBuildProjectile = false;
                            break;
                    }
                }

                float3 direction = math.normalizesafe(endPosition - startPosition);
                float distance = math.distance(endPosition, startPosition);
                foreach (BaseBehaviorSO behavior in skill.behaviors)
                {
                    switch (behavior.Type)
                    {
                        case BehaviourType.Spread:
                            SpreadBehaviourSO spread = behavior as SpreadBehaviourSO;
                            List<int> randomList = null;
                            if (spread.randomAngle)
                            {
                                randomList = new List<int>(spread.count);
                                for (int i = 0; i < spread.count; i++)
                                    randomList.Add(i);
                            }

                            for (int i = 0; i < spread.count; i++)
                            {
                                if (math.abs(spread.delayBetween) > 0)
                                {
                                    await UniTask.Delay(TimeSpan.FromSeconds(spread.delayBetween * i));
                                    Vector3 newEndPosition;
                                    if (spread.randomAngle)
                                    {
                                        int randomIndex = RandomUtils.Range(0, randomList.Count);
                                        newEndPosition = GetSpreadEndPosition(startPosition, direction,
                                            randomList[randomIndex], spread.count, spread.angleStep, distance
                                        );
                                        randomList.RemoveAt(randomIndex);
                                    }
                                    else
                                    {
                                        newEndPosition = GetSpreadEndPosition(
                                            startPosition, direction, i, spread.count, spread.angleStep, distance
                                        );
                                    }
                                    BuildProjectileInternal(manager, source, startPosition, newEndPosition, skill, statData);
                                }
                                else
                                {
                                    Vector3 newEndPosition = GetSpreadEndPosition(
                                        startPosition, direction, i, spread.count, spread.angleStep, distance
                                    );
                                    BuildProjectileInternal(manager, source, startPosition, newEndPosition, skill, statData);
                                }
                            }

                            break;
                        case BehaviourType.DropStrike:
                            DropStrikeBehaviourSO dropStrike = behavior as DropStrikeBehaviourSO;
                            float3 center = dropStrike.overrideCenter ? float3.zero : endPosition;
                            for (int i = 0; i < dropStrike.count; i++)
                            {
                                await UniTask.Delay(TimeSpan.FromSeconds(dropStrike.delayBetween));
                                float2 offsetRandom = UnityEngine.Random.insideUnitCircle * dropStrike.radius;
                                float3 finalEndPosition = center + new float3(offsetRandom.x, offsetRandom.y, 0);
                                float rad = math.radians(dropStrike.angle);
                                float3 dir = math.normalize(new float3(math.sin(rad), -math.cos(rad), 0));
                                float3 finalStartPosition = finalEndPosition - dir * dropStrike.height;
                                BuildProjectileInternal(manager, source, finalStartPosition, finalEndPosition, skill, statData);
                            }
                            break;
                        default:
                            if (shouldDefaultBuildProjectile)
                            {
                                BuildProjectileInternal(manager, source, startPosition, endPosition, skill, statData);
                            }
                            break;
                    }
                }
            }
        }
        
        static Vector3 GetSpreadEndPosition(Vector3 startPosition, Vector3 direction, int index, int total, float angle, float distance)
        {
            float angleStep = (index - (total - 1) * 0.5f) * angle;

            // xoay direction
            Quaternion rot = Quaternion.Euler(0, 0, angleStep);
            Vector3 dir = rot * direction;

            return startPosition + dir * distance;
        }

        static void BuildProjectileInternal(EntityManager manager, Entity source,
            float3 startPosition, float3 endPosition,
            Skill skill, SkillStatData statData)
        {
            GameObject go = KitPool.Instantiate(skill.projectile.prefab);
            go.transform.position = startPosition;
            
            float3 direction = endPosition - startPosition;
            float rad = Mathf.Atan2(direction.y, direction.x);
            quaternion rotation = quaternion.Euler(0, 0, rad);
            go.transform.rotation = rotation;
            
            EntityView view = go.GetComponent<EntityView>();
            Entity entity = view.GetOrCreateEntity();
            manager.AddComponentData(entity, new LocalTransform
            {
                Position = startPosition,
                Rotation = rotation,
                Scale = 1
            });
            
            float lifeTime = skill.main.lifeTime;
            switch (skill.trajectory.Type)
            {
                case TrajectoryType.Curve:
                    CurveTrajectorySO curve = skill.trajectory as CurveTrajectorySO;
                    manager.AddComponentData(entity,
                        new ProjectileCurveTrajectory(curve.curve, curve.value, lifeTime)
                    );
                    break;
                default:
                    Debug.LogError("Invalid trajectory type " + skill.trajectory.Type);
                    break;
            }

            /*manager.AddComponentData(entity,
                new ProjectileTrajectory(startPosition, direction)
            );*/
            
            manager.AddComponentObject(entity, view.transform);
#if UNITY_EDITOR
            manager.SetName(entity, go.name + "#" + entity.GetHashCode());
            view.name = go.name + "#" + entity.GetHashCode();
#endif
            manager.SetEnabled(entity, true);
            view.gameObject.SendMessage("Initialize", entity);
        }

        public static async UniTask BuildMonster(int monsterID, float bonusRange,
            float3 position)
        {
            Vector3 destination = float3.zero;
            KitConfigManager.Get<MonsterConfig>().Find(monsterID, out var monsterData);
            GameObject go = await KitLoaded.LoadAsync<GameObject>(monsterData.MonsterObjectId);
            if (monstersPath.Add(monsterData.MonsterObjectId))
            {
                KitPool.RegisterPool(go, true);
            }
            EntityView view = KitPool.Instantiate(go.GetComponent<EntityView>());
            view.transform.position = position;
            Monster monster = view.GetComponent<Monster>();
            float radius = monster.Radius;
            
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = view.GetOrCreateEntity();
            manager.AddComponentData(entity, new MonsterTag());
            manager.AddComponentData(entity, new MonsterFlipData
            {
                Changed = true,
                FacingRight = position.x >= destination.x,
            });
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
                StoppingDistance = bonusRange + monsterData.StopMoveDistance + radius,
                AutoBreaking = true,
            });
            manager.AddComponentData(entity, new AgentSeparation
            {
                Radius = radius, Weight = 1,
                Layers = monsterData.IsRanged ? NavigationLayers.Layer1 : NavigationLayers.Default
            });
            manager.AddComponentData(entity, new AgentShape
            {
                Radius = radius, Type = ShapeType.Circle,
            });
            manager.AddComponentData(entity, new AgentSonarAvoid
            {
                Radius = radius * 2,
                Mode = SonarAvoidMode.IgnoreBehindAgents,
                MaxAngle = math.radians(360),
                Angle = math.radians(180),
                BlockedStop = true,
                Layers = monsterData.IsRanged ? NavigationLayers.Default : NavigationLayers.Layer1
            });
            manager.AddComponentData(entity,
                new DefaultStatData(monsterData.MoveSpeed, monsterData.Attack, monsterData.Health)
            );
            manager.AddComponentData(entity, new HealthData
            {
                Health = monsterData.Health,
                MaxHealth = monsterData.Health,
            });
            manager.AddComponentData(entity,
                new MonsterSkillData(monsterID, monsterData.SkillId, monsterData.SkillLevel,
                    monsterData.SkillCooldown, bonusRange + monsterData.AttackDistance + radius, 1)
            );
            if (monsterData.IsRanged)
                manager.AddComponentData(entity, new MonsterRangedTag());
            else
                manager.AddComponentData(entity, new MonsterMeleeTag());
            manager.AddComponentObject(entity, view.transform);
#if UNITY_EDITOR
            manager.SetName(entity, go.name + "#" + entity.GetHashCode());
            view.name = go.name + "#" + entity.GetHashCode();
#endif
            manager.SetEnabled(entity, true);
            view.gameObject.SendMessage("Initialize", entity);
            await UniTask.CompletedTask;
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