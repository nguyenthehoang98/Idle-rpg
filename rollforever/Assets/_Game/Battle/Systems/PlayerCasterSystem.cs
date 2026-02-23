
using System.Collections.Generic;
using _Game.Battle.Data;
using _Game.Battle.Events;
using _Game.Battle.Model;
using _Game.Scripts.Configs;
using _KIT.Config;
using _KIT.Event;
using Geometry;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class PlayerCasterSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private Dictionary<int, int> mapSlotEntities = new Dictionary<int, int>();
        private WeaponConfig weaponConfig;
        private Vector2[] points;
        private int currentPoint;
        private float elapsed;
        private float duration = 1f;

        private EcsPool<StatData> statPool;
        private EcsPool<WeaponCasterData> weaponCasterPool;
        private EcsFilter weaponFilter;
        private EcsWorld world;

        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            weaponFilter = world.Filter<WeaponCasterData>()
                .End();
            weaponCasterPool = world.GetPool<WeaponCasterData>();
            statPool = world.GetPool<StatData>();
            
            weaponConfig = KitConfigManager.Get<WeaponConfig>();
            points = shareData.LevelSpawnConfig.designConfig.LoopPoints();
        }

        public void Run(IEcsSystems systems)
        {
            elapsed += shareData.TimeDelta;
            Vector2 cur = points[currentPoint];
            Vector2 target = points[0];
            if (currentPoint + 1 < points.Length)
            {
                target = points[currentPoint + 1];
            }

            Vector2 position = Vector2.Lerp(cur, target, elapsed / duration);
            if (elapsed >= duration)
            {
                currentPoint++;
                if (currentPoint >= points.Length) currentPoint = 0;
                elapsed = 0;

                if (mapSlotEntities.TryGetValue(currentPoint, out int entity))
                {
                    ref var caster = ref weaponCasterPool.Get(entity);
                    EventBus.Instance.Publish(
                        new CastSkillEvent(entity, caster.skillId, caster.startPosition, Team.Player)
                    );
#if UNITY_EDITOR && (DEVELOP_MODE || COMBAT_FULL_LOG)
                    GeometryGizmos.DrawObb(
                        new OBB(caster.startPosition, new float2(0.4f, 0.4f), new float2(1, 0), new float2(0, 1)),
                        Color.red, shareData.TimeDelta
                    );
#endif
                }
            }
            
#if UNITY_EDITOR && (DEVELOP_MODE || COMBAT_FULL_LOG)
            GeometryGizmos.DrawCircle(
                new Circle(position, 0.45f),
                Color.yellow, shareData.TimeDelta
            );
#endif
            
#if UNITY_EDITOR && (DEVELOP_MODE || COMBAT_FULL_LOG)
            foreach (var e in weaponFilter)
            {
                var caster = weaponCasterPool.Get(e);
                GeometryGizmos.DrawBox(
                    Box.FromCenter(caster.startPosition, new float2(1, 1)),
                    Color.magenta, shareData.TimeDelta
                );
            }
#endif
        }
        
        public void Equip(int slotId, int weaponId, int level)
        {
            bool flag1 = weaponConfig.Find(weaponId, out WeaponConfig.WeaponData weaponData);
            if (!flag1)
            {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                Debug.LogError($"Not found Weapon with id '{weaponId}'");
#endif
                return;
            }

            Vector2 position = shareData.LevelSpawnConfig.designConfig.LoopPoints()[slotId];
            
            int entity = world.NewEntity();
            weaponCasterPool.Add(entity) = new WeaponCasterData
            {
                skillId = weaponData.SkillId,
                startPosition = position,
            };
            statPool.Add(entity) = new StatData()
                .Insert(StatType.Attack, new Stat(weaponData.Attack(level)))
                .Insert(StatType.CriticalRate, new Stat(0))
                .Insert(StatType.CriticalDamage, new Stat(0))
                .Insert(StatType.AttackPercent, new Stat(0));
            
            mapSlotEntities[slotId] = entity;
            EventBus.Instance.Publish(new EquipEquipmentEvent(slotId, weaponId, level));
        }

        // Cần xóa stat cũ -> stat mới.
        public void UpgradeStat(List<BuffConfig.BuffData> buffDatas)
        {
            foreach (var e in weaponFilter)
            {
                ref var stat = ref statPool.Get(e);
                stat.ReplaceModifier(buffDatas);
            }
        }
    }
}