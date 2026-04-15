using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.EntityComponentSystem.Model;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem.Config;
using _Games.Combat.SkillSystem.Model;
using _Games.Config;
using _Games.Utils;
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
using Projectile = _Games.Combat.Model.Projectile;

namespace _Games.Combat.EntityComponentSystem
{
    public static class ECSFactory
    {
        private static HashSet<string> monstersPath = new HashSet<string>();
        
        public static Entity BuildPlayer(LevelDesign levelDesign)
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = manager.CreateEntity();
            DynamicBuffer<CircleBuffer> circleBuffers = manager.AddBuffer<CircleBuffer>(entity);
            circleBuffers.Add(new CircleBuffer(levelDesign.Radius, float3.zero, false, 0));
            manager.AddComponentData(entity, new PlayerTag());
            manager.AddComponentData(entity, new LocalTransform { Position = float3.zero });
            int health = 500;
            manager.AddComponentData(entity, new HealthData { Health = health, MaxHealth = health });
#if UNITY_EDITOR
            manager.SetName(entity, "Player");
#endif
            return entity;
        }

        public static async UniTask BuildMonster(Entity player, int monsterID, float bonusRange, float3 position)
        {
            Vector3 destination = float3.zero;
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            MonsterConfig monsterConfig = KitConfigManager.Get<MonsterConfig>();
            bool foundMonsterData = monsterConfig.Find(monsterID, out var monsterData);
#if DEBUG
            if(!foundMonsterData) Debug.LogError("Not found monster data: " + monsterID);
#endif
            if (!foundMonsterData) return;

            string path = GlobalsPath.GetMonsterPath(monsterData.MonsterId);
            GameObject go = await KitLoaded.LoadAsync<GameObject>(path);
            MonsterAuthoring authoringPrefab = go.GetComponent<MonsterAuthoring>();
#if !TEST_MODE
            if (monstersPath.Add(path))
            {
                KitPool.RegisterPool(go, true);
            }
#endif
            
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            MonsterAuthoring authoring = null;
            Entity entity;
#if !TEST_MODE
            EntityView view = KitPool.Instantiate(go.GetComponent<EntityView>());
            view.transform.position = position;
            view.transform.localRotation = Quaternion.Euler(0, 0, 0);
            view.transform.localScale = Vector3.one;
            authoring = view.GetComponent<MonsterAuthoring>();
            entity = view.GetOrCreateEntity();
#else
            entity = manager.CreateEntity();
#endif
            Monster monster = null;
            RangedMonsterAuthoring rangedMonsterAuthoring = authoringPrefab.GetComponent<RangedMonsterAuthoring>();
            if (rangedMonsterAuthoring != null)
            {
                RangedMonster rangedMonster = ScriptableObject.CreateInstance<RangedMonster>();
                rangedMonster.Init(authoring, rangedMonsterAuthoring.MuzzleOffset, entity, player, monsterID,
                    monsterData.SkillId, monsterData.SkillLevel, monsterConfig, skillConfig, authoringPrefab.DelayExecuteAttack);
                monster = rangedMonster;
            }
            else
            {
                monster = ScriptableObject.CreateInstance<Monster>();
                monster.Init(authoring, entity, player, monsterID,
                    monsterData.SkillId, monsterData.SkillLevel, monsterConfig, skillConfig, authoringPrefab.DelayExecuteAttack);
            }

            if (monster == null) Debug.LogError("Monster undefined " + monsterData.MonsterId);
            
            float radius = authoringPrefab.Radius;
            DynamicBuffer<CircleBuffer> circleBuffers = manager.AddBuffer<CircleBuffer>(entity);
            circleBuffers.Add(new CircleBuffer(radius, float3.zero, false, 0));
            manager.AddComponentData(entity, new MonsterTag());
            manager.AddComponentData(entity, new LocalTransform
            {
                Position = position, Rotation = quaternion.RotateY(math.radians(position.x < destination.x ? 0 : 180)), Scale = 1
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
                BaseSpeed = monsterData.MoveSpeed,
                Speed = monsterData.MoveSpeed * BattleTime.ScaleTime,
                Acceleration = monsterData.MoveSpeed, 
                AngularSpeed = 0,
                StoppingDistance = bonusRange + monsterData.StopMoveDistance + radius,
                AutoBreaking = true,
            });
            manager.AddComponentData(entity, new AgentSeparation
            {
                Radius = radius, Weight = 10,
                Layers = monsterData.IsRanged ? NavigationLayers.Layer1 : NavigationLayers.Default
            });
            manager.AddComponentData(entity, new AgentShape
            {
                Radius = radius, Type = ShapeType.Circle,
            });
            manager.AddComponentData(entity, new AgentSonarAvoid
            {
                Radius = radius * 5,
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
            manager.AddComponentObject(entity, monster);
            manager.SetEnabled(entity, true);
#if !TEST_MODE
            manager.AddComponentObject(entity, authoring.transform);
            view.gameObject.SendMessage("Initialize", entity);
#endif
#if UNITY_EDITOR && !TEST_MODE
            manager.SetName(entity, go.name + "#" + entity.GetHashCode());
            view.name = go.name + "#" + entity.GetHashCode();
#endif
            await UniTask.CompletedTask;
        }
        
        public static async void BuildProjectile(EntityManager manager, Entity source,
            float3 startPosition, float3 endPosition, Skill skill, SkillData skillData, int level)
        {
            if (skill.behaviors.Length == 0)
            {
                BuildProjectileInternal(manager, source, startPosition, endPosition, skill, skillData, level);
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
                                    BuildProjectileInternal(manager, source, startPosition, newEndPosition, skill, skillData, level);
                                }
                                else
                                {
                                    Vector3 newEndPosition = GetSpreadEndPosition(
                                        startPosition, direction, i, spread.count, spread.angleStep, distance
                                    );
                                    BuildProjectileInternal(manager, source, startPosition, newEndPosition, skill, skillData, level);
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
                                BuildProjectileInternal(manager, source, finalStartPosition, finalEndPosition, skill, skillData, level);
                            }
                            break;
                        default:
                            if (shouldDefaultBuildProjectile)
                            {
                                BuildProjectileInternal(manager, source, startPosition, endPosition, skill, skillData, level);
                            }
                            break;
                    }
                }
            }
        }
        
        private static Vector3 GetSpreadEndPosition(Vector3 startPosition, Vector3 direction, int index, int total, float angle, float distance)
        {
            float angleStep = (index - (total - 1) * 0.5f) * angle;

            // xoay direction
            Quaternion rot = Quaternion.Euler(0, 0, angleStep);
            Vector3 dir = rot * direction;

            return startPosition + dir * distance;
        }

        private static void BuildProjectileInternal(EntityManager manager, Entity source,
            float3 startPosition, float3 endPosition,
            Skill skill, SkillData skillData, int level)
        {
            float3 direction = endPosition - startPosition;
            float rad = Mathf.Atan2(direction.y, direction.x);
            quaternion rotation = quaternion.Euler(0, 0, rad);
            
            ProjectileAuthoring authoring = null;
            EntityView view = null;
            Entity entity;
#if !TEST_MODE
            if (skill.projectile.prefab != null)
            {
                authoring = KitPool.Instantiate(skill.projectile.prefab).GetComponent<ProjectileAuthoring>();
                authoring.transform.position = startPosition;
                authoring.transform.rotation = rotation;
                view = authoring.GetComponent<EntityView>();
                entity = view.GetOrCreateEntity();
            }
            else
            {
                Debug.LogError("Skill projectile prefab is null: " + skillData.SkillId);
                return;
            }

#else
            entity = manager.CreateEntity();  
#endif
            Projectile projectile = ScriptableObject.CreateInstance<Projectile>();
            projectile.Init(authoring);
            manager.AddComponentData(entity, new ProjectileTag());
            manager.AddBuffer<CollisionBuffer>(entity);
            manager.AddComponentData(entity, new LocalTransform
            {
                Position = startPosition,
                Rotation = rotation,
                Scale = 1
            });

            SkillMainModule main = skill.main;
            float lifeTime = main.lifeTime;
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

            switch (skill.collider.Type)
            {
                case ColliderType.Circle:
                    var circleBuffers = manager.AddBuffer<CircleBuffer>(entity);
                    CircleColliderSO circle = skill.collider as CircleColliderSO;
                    foreach (var c in circle.list)
                    {
                        circleBuffers.Add(new CircleBuffer(c.radius, c.offset, c.adjustRadius, c.extraRadius));
                    }
                    break;
                default:
                    Debug.LogError("Invalid collider type " + skill.collider.Type);
                    break;
            }

            manager.AddComponentData(entity,
                new ProjectileTrajectory(startPosition, math.normalizesafe(direction))
            );
            manager.AddComponentData(entity,
                new ProjectileSkillData(source, skill.Id, lifeTime,
                    main.maxHitCount, main.collisionResetInterval,
                    skillData.FlatDamage(level), skillData.ScaleDamage(level))
            );

            manager.AddComponentObject(entity, projectile);
            manager.SetEnabled(entity, true);
#if !TEST_MODE
            manager.AddComponentObject(entity, view.transform);
            view.gameObject.SendMessage("Initialize", entity);
#endif
#if UNITY_EDITOR && !TEST_MODE
            manager.SetName(entity, authoring.name + "#" + entity.GetHashCode());
            view.name = authoring.name + "#" + entity.GetHashCode();
#endif
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