using System;
using System.Collections;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.SkillSystem.Model;
using _KIT.Config;
using _KIT.Resource;
using _KIT.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class TriggerWeaponLogic : IDisposable
    {
        LevelDesign levelDesign;
        //Dictionary<int, Data> container = new Dictionary<int, Data>();
        Queue<Action> queueBuffer = new Queue<Action>();
        SkillConfig skillConfig;
        private EntityQuery query;
        
        public TriggerWeaponLogic(LevelDesign levelDesign)
        {
            skillConfig = KitConfigManager.Get<SkillConfig>();
            this.levelDesign = levelDesign;
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(LocalTransform), typeof(HealthData));
        }

        /*public void Equip(int slotIndex, WeaponConfig.WeaponData weaponData, int level)
        {
            container[slotIndex] = new Data
            {
                WeaponData = weaponData,
                Level = level
            };
            Debug.Log(@"Thêm cơ chế lock mục tiêu tới khi chết, tránh đảo nhiều quá ko hay. Weapon lock target chứ ko phải player lock");
        }*/

        public async void Trigger(int slotIndex)
        {
            /*float3 position = levelDesign.Slots[slotIndex].transform.position;

            if (!container.ContainsKey(slotIndex))
                return; // ko có vũ khí
            Data data = container.GetValueOrDefault(slotIndex);
            bool foundSkill = skillConfig.Find(data.WeaponData.SkillId, data.Level, out var skillData);
            if (!foundSkill)
            {
                Debug.LogError("Not found skill: " + data.WeaponData.SkillId + "_" + data.Level);
            }

            AbilitySO ability = await KitLoaded.LoadAsync<AbilitySO>(skillData.SkillPath);

            // todo: query
            bool found = false;
            var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var healths = query.ToComponentDataArray<UnitHealth>(Allocator.TempJob);
            var nearestIndex = new NativeReference<int>(-1, Allocator.TempJob);
            var farthestIndex = new NativeReference<int>(-1, Allocator.TempJob);

            float thresholdDistance = float.MaxValue;
            switch (ability.findTarget.type)
            {
                case FindTargetType.Farthest:
                    thresholdDistance = ability.findTarget.farthest.distance;
                    break;
                case FindTargetType.Nearest:
                    thresholdDistance = ability.findTarget.nearest.distance;
                    break;
            }


            switch (ability.findTarget.type)
            {
                case FindTargetType.Farthest:
                case FindTargetType.Nearest:
                    var job = new QueryMonsterJob
                    {
                        ThresholdDistanceSq = thresholdDistance * thresholdDistance,
                        TargetPoint = position,
                        Transforms = transforms,
                        Healths = healths,
                        FarthestIndex = farthestIndex,
                        NearestIndex = nearestIndex,
                    };
                    var handle = job.Schedule();
                    handle.Complete();
                    break;
            }

            float3 endPosition = float3.zero;
            switch (ability.findTarget.type)
            {
                case FindTargetType.Farthest:
                    if (farthestIndex.Value >= 0)
                    {
                        endPosition = transforms[farthestIndex.Value].Position;
                        found = true;
                    }
                    else
                        endPosition = position + new float3(RandomUtils.Range(-5, 5), RandomUtils.Range(-5, 5), 0f);

                    break;
                case FindTargetType.Nearest:
                    if (nearestIndex.Value >= 0)
                    {
                        endPosition = transforms[nearestIndex.Value].Position;
                        found = true;
                    }
                    else
                        endPosition = position + new float3(RandomUtils.Range(-5, 5), 0f, 0f);

                    break;
            }

            transforms.Dispose();
            nearestIndex.Dispose();
            farthestIndex.Dispose();

            if (!found && ability.findTarget.requireTarget) return;

            float delay = 0.2f;
            float attack = data.WeaponData.Attack(data.Level);
            float3 direction = endPosition - position;
            float rad = Mathf.Atan2(direction.y, direction.x);
            levelDesign.Slots[slotIndex].Rotation(rad, delay);
            levelDesign.StartCoroutine(Push(() =>
            {
                EntityFactory.BuildProjectile(Entity.Null, position, endPosition, false,
                    (int)attack, int.Parse(skillData.SkillPath), ability
                );
            }, delay));*/
        }

        IEnumerator Push(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            queueBuffer.Enqueue(action);
        }

        public void Update()
        {
            while (queueBuffer.Count > 0)
            {
                queueBuffer.Dequeue().Invoke();
            }
        }
        
        public void Dispose()
        {
        }
        
        /*[BurstCompile]
        struct QueryMonsterJob : IJob
        {
            [ReadOnly] public NativeArray<LocalTransform> Transforms;
            [ReadOnly] public NativeArray<UnitHealth> Healths;
            [ReadOnly] public float3 TargetPoint;
            [ReadOnly] public float ThresholdDistanceSq;

            public NativeReference<int> NearestIndex;
            public NativeReference<int> FarthestIndex;

            public void Execute()
            {
                float maxDist = float.MinValue;
                float minDist = float.MaxValue;
                int nearest = -1;
                int farthest = -1;

                for (int i = 0; i < Transforms.Length; i++)
                {
                    if (Healths[i].CurrentHealth <= 0) continue;
                    float3 pos = Transforms[i].Position;
                    float dist = math.distancesq(pos, TargetPoint);
                    if (dist > ThresholdDistanceSq) continue;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = i;
                    }

                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        farthest = i;
                    }
                }

                FarthestIndex.Value = farthest;
                NearestIndex.Value = nearest;
            }
        }

        struct Data
        {
            public WeaponConfig.WeaponData WeaponData;
            public int Level;
        }*/
    }
}