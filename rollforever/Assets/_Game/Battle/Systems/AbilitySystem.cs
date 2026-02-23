using System.Collections.Generic;
using _Game.Battle.AbilitySystem;
using _Game.Battle.Data;
using _Game.Battle.Events;
using _Game.Battle.Model;
using _Game.Scripts.Configs;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Cysharp.Threading.Tasks;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class AbilitySystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private EcsWorld world;
        private EcsFilter monsterFilter;
        private EcsFilter playerFilter;

        // model
        private Dictionary<int, AbilityLogic> skillSource;
        private Dictionary<FindTargetType, IFindTarget> findTargets;
        private List<AbilityLogic> abilities;
        private Queue<AbilityLogic> additions;
        private Queue<AbilityLogic> completes;

        public async void Init(IEcsSystems systems)
        {
            additions = new Queue<AbilityLogic>();
            completes = new Queue<AbilityLogic>();
            abilities = new List<AbilityLogic>();

            world = systems.GetWorld();
            monsterFilter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            playerFilter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .End();

            var unitPool = world.GetPool<UnitData>();
            var shapePool = world.GetPool<ShapeData>();
            var deadPool = world.GetPool<DeadFlag>();
            var healthPool = world.GetPool<HealthData>();
            var statPool = world.GetPool<StatData>();
            var modifierPool = world.GetPool<UnitModifierData>();
            var unitPosTempPool = world.GetPool<UnitPosTempData>();

            Dictionary<int, string> allSkills = new Dictionary<int, string>();
            MonsterConfig monsterConfig = KitConfigManager.Get<MonsterConfig>();
            foreach (var wave in shareData.LevelSpawnConfig.waves)
            {
                foreach (var batch in wave.batches)
                {
                    foreach (var enemy in batch.enemies)
                    {
                        if (!monsterConfig.Find(enemy.id, out var monsterData)) continue;
                        if (allSkills.ContainsKey(monsterData.SkillId)) continue;
                        allSkills.Add(monsterData.SkillId, GetSkillAddressPath(monsterData.SkillId));
                    }
                }
            }

            List<int> playerSkillsId = new List<int> { 20101 };
            foreach (var skillId in playerSkillsId)
            {
                allSkills.Add(skillId, GetSkillAddressPath(skillId));
            }

            findTargets = new Dictionary<FindTargetType, IFindTarget>();
            findTargets.Add(FindTargetType.Farthest, new FarthestFindTarget(shareData.Simulator, unitPool));
            findTargets.Add(FindTargetType.Nearest, new NearestFindTarget(shareData.Simulator, unitPool));
            skillSource = await BuildAbilities(
                allSkills, shareData, runtimeData, unitPool, shapePool, deadPool,
                healthPool, statPool, modifierPool, unitPosTempPool, playerFilter
            );
            EventBus.Instance.Subscribe<CastSkillEvent>(OnCastSkillArg);
        }

        private void OnCastSkillArg(CastSkillEvent e)
        {
            if (skillSource.TryGetValue(e.SkillId, out var ability))
            {
                if (findTargets.TryGetValue(ability.AbilityData.core.findTarget, out var findTarget))
                {
                    EcsFilter filter = e.Team == Team.Player ? monsterFilter : playerFilter;
                    bool found = findTarget.Find(e.StartPosition,
                        ability.AbilityData.core.maxDistanceFindTarget,
                        filter, out int target
                    );

                    if (found || !ability.AbilityData.core.isRequireTarget)
                    {
                        AbilityLogic abilityInstance = ability.CreateInstance(e.Source, e.Team);
                        abilityInstance.Startup(e.Source, e.StartPosition, found, target);
                        additions.Enqueue(abilityInstance);
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
                        AbilityDebugView.Create(abilityInstance);
#endif                        
                    }
                }
                else
                {
#if DEVELOP_MODE
                    Debug.LogError($"'{ability.AbilityData.core.findTarget}' not registered to system, skill source '{e.SkillId}'");
#endif
                }
            }
            else
            {
#if DEVELOP_MODE
                Debug.LogError($"Skill '{e.SkillId}' not registered");
#endif
            }
        }

        public void Run(IEcsSystems systems)
        {
            while (additions.Count > 0)
            {
                var item = additions.Dequeue();
                world.AddEventListener(item);
                abilities.Add(item);
            }

            float dt = shareData.TimeDelta;
            foreach (var ability in abilities)
            {
                ability.Update(dt);
            }

            foreach (var ability in abilities)
            {
                if (ability.IsCompleted) completes.Enqueue(ability);
            }

            while (completes.Count > 0)
            {
                var item = completes.Dequeue();
                item.Shutdown();
                item.Dispose();
                world.RemoveEventListener(item);
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
                AbilityDebugView.Release(item);
#endif
                CollectionUtils.RemoveFast(abilities, item);
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            EventBus.Instance.Unsubscribe<CastSkillEvent>(OnCastSkillArg);
        }

        public void ClearAll()
        {
            while (additions.Count > 0)
            {
                completes.Enqueue(additions.Dequeue());
            }

            foreach (var ability in abilities)
            {
                completes.Enqueue(ability);
            }
            
            while (completes.Count > 0)
            {
                var item = completes.Dequeue();
                item.Shutdown();
                item.Dispose();
                world.RemoveEventListener(item);
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
                AbilityDebugView.Release(item);
#endif
                CollectionUtils.RemoveFast(abilities, item);
            }
        }

        private static async UniTask<Dictionary<int, AbilityLogic>> BuildAbilities(
            Dictionary<int, string> abilitiesPath, BattleStartupShareData shareData,
            BattleStartupRuntimeData runtimeData,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool,
            EcsPool<HealthData> healthPool, EcsPool<StatData> statPool, EcsPool<UnitModifierData> modifierPool,
            EcsPool<UnitPosTempData> unitPosTempPool,
            EcsFilter playerFilter)
        {
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            Dictionary<int, AbilityLogic> dict = new Dictionary<int, AbilityLogic>();
            foreach (var (abilityId, path) in abilitiesPath)
            {
                if (!skillConfig.Find(abilityId, out var skillData))
                {
#if DEVELOP_MODE
                    Debug.LogError($"Không tìm thấy SkillData với id '{abilityId}'");
#endif
                    continue;
                }

                AbilityData abilityData = await KitLoaded.LoadAsync<AbilityData>(path);
                dict[abilityId] = new AbilityLogic(abilityData, skillData, 0, Team.Player, shareData, runtimeData,
                    unitPool, shapePool, deadPool, healthPool, statPool, modifierPool, unitPosTempPool, playerFilter
                );

                if (abilityData.core.bulletPrefab != null)
                {
                    KitPool.RegisterPool(abilityData.core.bulletPrefab.gameObject, true);
                }
            }

            return dict;
        }

        private static string GetSkillAddressPath(int skillId)
        {
            return string.Format("Skill_{0}", skillId);
        }
    }
}