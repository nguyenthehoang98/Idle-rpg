using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Level;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Config;
using _KIT.Config;
using _KIT.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.Equipment
{
    public class EquipmentManager
    {
        private LevelDesign levelDesign;
        private Dictionary<int, Data> container = new Dictionary<int, Data>();
        private SkillConfig skillConfig;
        private EntityManager manager;
        private EntityQuery query;
        
        public EquipmentManager(LevelDesign levelDesign)
        {
            skillConfig = KitConfigManager.Get<SkillConfig>();
            this.levelDesign = levelDesign;
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(LocalTransform), typeof(HealthData));
        }

        public void Equip(int slotIndex, WeaponData weaponData, int level)
        {
            container[slotIndex] = new Data
            {
                WeaponData = weaponData,
                Level = level
            };
            Debug.Log(@"Thêm cơ chế lock mục tiêu tới khi chết, tránh đảo nhiều quá ko hay. Weapon lock target chứ ko phải player lock");
        }

        public async void Trigger(int slotIndex)
        {
            Vector3 position = levelDesign.Slots[slotIndex].transform.position;
            if (!container.ContainsKey(slotIndex))
                return; // ko có vũ khí
            Data data = container.GetValueOrDefault(slotIndex);
            bool foundSkill = skillConfig.Find(data.WeaponData.SkillId, out var skillData);
            if (!foundSkill)
            {
                Debug.LogError("Not found skill: " + data.WeaponData.SkillId + "_" + data.Level);
                return;
            }

            Skill skill = await SkillFactory.CreateSkill(skillData);

            // todo: query
            bool found = false;
            var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var healths = query.ToComponentDataArray<HealthData>(Allocator.TempJob);
            var nearestIndex = new NativeReference<int>(-1, Allocator.TempJob);
            var farthestIndex = new NativeReference<int>(-1, Allocator.TempJob);

            float thresholdDistance = float.MaxValue;
            switch (skill.main.type)
            {
                case FindTargetType.Farthest:
                case FindTargetType.Nearest:
                    thresholdDistance = skill.main.maxTargetRange;
                    break;
            }

            switch (skill.main.type)
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

            Vector3 endPosition = Vector3.zero;
            switch (skill.main.type)
            {
                case FindTargetType.Farthest:
                    if (farthestIndex.Value >= 0)
                    {
                        endPosition = transforms[farthestIndex.Value].Position;
                        found = true;
                    }
                    else
                        endPosition = position + new Vector3(RandomUtils.Range(-5, 5), RandomUtils.Range(-5, 5), 0f);

                    break;
                case FindTargetType.Nearest:
                    if (nearestIndex.Value >= 0)
                    {
                        endPosition = transforms[nearestIndex.Value].Position;
                        found = true;
                    }
                    else
                        endPosition = position + new Vector3(RandomUtils.Range(-5, 5), 0f, 0f);

                    break;
            }

            transforms.Dispose();
            nearestIndex.Dispose();
            farthestIndex.Dispose();

            if (!found && skill.main.needTargetToCast)
                return;

            float delay = 0.2f / BattleTime.ScaleTime;
            Vector3 direction = endPosition - position;
            float rad = Mathf.Atan2(direction.y, direction.x);
            levelDesign.Slots[slotIndex].Rotation(rad, delay, () =>
            {
                ECSFactory.BuildProjectile(manager, Entity.Null, position, endPosition, skill, skillData, data.Level);
            });
        }
        
        [BurstCompile]
        struct QueryMonsterJob : IJob
        {
            [ReadOnly] public NativeArray<LocalTransform> Transforms;
            [ReadOnly] public NativeArray<HealthData> Healths;
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
                    if (Healths[i].Health <= 0) continue;
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
            public int Level;
            public WeaponData WeaponData;
        }
    }
}