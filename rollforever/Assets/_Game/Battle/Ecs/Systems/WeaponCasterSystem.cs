using System.Collections.Generic;
using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Model;
using _Game.Battle.Level;
using _Game.Battle.UI;
using _Game.Scripts.Configs;
using _Game.Scripts.Model;
using _KIT.Config;
using _KIT.Event;
using _KIT.Resource;
using Geometry;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.Systems
{
    public class WeaponCasterSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostDestroySystem
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

        private Transform playerInstance;
        private LevelDesignConfig levelDesignInstance; 

        public async void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            weaponFilter = world.Filter<WeaponCasterData>()
                .Inc<StatData>()
                .End();

            var weaponFlagPool = world.GetPool<WeaponFlag>();
            weaponCasterPool = world.GetPool<WeaponCasterData>();
            statPool = world.GetPool<StatData>();
            
            weaponConfig = KitConfigManager.Get<WeaponConfig>();
            points = shareData.LevelSpawnSo.designConfig.LoopPoints();

            levelDesignInstance = Object.Instantiate(shareData.LevelSpawnSo.designConfig);
            levelDesignInstance.transform.position = Vector3.zero;

            GameObject go = await KitLoaded.LoadAsync<GameObject>("Player");
            playerInstance = Object.Instantiate(go).transform;
            playerInstance.position = new Vector3(points[0].x, points[0].y);
            playerInstance.gameObject.SetActive(false);

            // todo: build slots
            for (int i = 0; i < points.Length; i++)
            {
                int entity = world.NewEntity();
                weaponFlagPool.Add(entity);
                statPool.Add(entity) = new StatData()
                    .Insert(StatType.Attack, new Stat(0))
                    .Insert(StatType.CriticalRate, new Stat(0))
                    .Insert(StatType.CriticalDamage, new Stat(0))
                    .Insert(StatType.AttackPercent, new Stat(0));
                mapSlotEntities[i] = entity;
            }
            
            EventBus.Instance.Subscribe<WaveCompleteEvent>(OnWaveComplete);
            EventBus.Instance.Subscribe<WaveResumeEvent>(OnWaveResume);
            EventBus.Instance.Subscribe<WaveChooseBuffEvent>(OnWaveChooseBuff);
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
            
            if(playerInstance != null)
                playerInstance.position = position;
            
            if (elapsed >= duration)
            {
                currentPoint++;
                if (currentPoint >= points.Length) currentPoint = 0;
                elapsed = 0;

                if (mapSlotEntities.TryGetValue(currentPoint, out int entity))
                {
                    if (weaponCasterPool.Has(entity))
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

        public void PostDestroy(IEcsSystems systems)
        {
            EventBus.Instance.Unsubscribe<WaveCompleteEvent>(OnWaveComplete);
            EventBus.Instance.Unsubscribe<WaveResumeEvent>(OnWaveResume);
            EventBus.Instance.Unsubscribe<WaveChooseBuffEvent>(OnWaveChooseBuff);
        }

        private void OnWaveComplete(WaveCompleteEvent obj)
        {
            SetActivePlayer(false);
        }

        private void OnWaveChooseBuff(WaveChooseBuffEvent e)
        {
            foreach (var entity in weaponFilter)
            {
                ref var stat = ref statPool.Get(entity);
                stat.ReplaceModifier(e.Buffs);
                EventBus.Instance.Publish(new EntityChangedStatEvent(entity));
            }
        }

        private void OnWaveResume(WaveResumeEvent e)
        {
            // todo: update eqm
            var equipments = levelDesignInstance.AllEquipments();
            for (int i = 0; i < equipments.Length; i++)
            {
                int entity = mapSlotEntities[i];
                EquipmentItem equipment = equipments[i];
                if (equipment == null)
                    continue;

                int level = equipment.WeaponLevel;
                int weaponId = equipment.WeaponData.WeaponId;
                bool flag1 = weaponConfig.Find(weaponId, out WeaponConfig.WeaponData weaponData);
                if (!flag1)
                {
#if DEVELOP_MODE || COMBAT_FULL_LOG
                    Debug.LogError($"Not found Weapon with id '{weaponId}'");
#endif
                    continue;
                }
                
                Vector2 position = equipment.transform.parent.position;
                WeaponCasterData casterData = new WeaponCasterData
                {
                    skillId = weaponData.SkillId,
                    startPosition = position,
                };
                if (weaponCasterPool.Has(entity))
                {
                    weaponCasterPool.Get(entity) = casterData;
                }
                else
                {
                    weaponCasterPool.Add(entity) = casterData;
                }

                ref var stat = ref statPool.Get(entity);
                stat.Replace(StatType.Attack, new Stat(weaponData.Attack(level)));
                
                EventBus.Instance.Publish(new UpdateEquipmentEvent(i, weaponId, level));
            }

            SetActivePlayer(true);
        }

        void SetActivePlayer(bool active)
        {
            if (playerInstance != null) playerInstance.gameObject.SetActive(active);
        }
    }
}