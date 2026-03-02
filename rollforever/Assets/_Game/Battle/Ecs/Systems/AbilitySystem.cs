using System.Collections.Generic;
using System.Linq;
using _Game.Battle.AbilitySystem;
using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Model;
using _Game.Scripts.Configs;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Cysharp.Threading.Tasks;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.Systems
{
    public class AbilitySystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsWorld world;
        private EcsFilter monsterFilter;
        private EcsFilter playerFilter;
        private EcsPool<MonsterAgentData> unitPool;
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
            monsterFilter = world.Filter<MonsterAgentData>()
                .Inc<MonsterFlag>()
                .Exc<PlayerFlag>()
                .Exc<DeadFlag>()
                .End();
            playerFilter = world.Filter<StatData>()
                .Inc<HealthData>()
                .Inc<ShapeData>()
                .Inc<PlayerFlag>()
                .Exc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();

            unitPool = world.GetPool<MonsterAgentData>();
            var shapePool = world.GetPool<ShapeData>();
            var deadPool = world.GetPool<DeadFlag>();
            var healthPool = world.GetPool<HealthData>();
            var statPool = world.GetPool<StatData>();
            var modifierPool = world.GetPool<UnitModifierData>();
            var unitPosTempPool = world.GetPool<MonsterTempData>();

            Dictionary<int, string> allSkills = new Dictionary<int, string>();
            MonsterConfig monsterConfig = KitConfigManager.Get<MonsterConfig>();
            foreach (var wave in shareData.LevelSpawnSo.waves)
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

            WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
            int[] keys = weaponConfig.AllKeys;
            foreach (var key in keys)
            {
                if (weaponConfig.Find(key, out var weaponData))
                {
                    int skillId = weaponData.SkillId;
                    allSkills.Add(skillId, GetSkillAddressPath(skillId));
                }
            }

#if DEVELOP_MODE || COMBAT_FULL_LOG
            int[] ids = allSkills.Keys.ToArray();
            Debug.Log($"Abilities Loaded: " + string.Join(',', ids));
#endif

            findTargets = new Dictionary<FindTargetType, IFindTarget>();
            findTargets.Add(FindTargetType.Farthest, new FarthestFindTarget());
            findTargets.Add(FindTargetType.Nearest, new NearestFindTarget());
            skillSource = await BuildAbilities(
                allSkills, shareData, unitPool, shapePool, deadPool,
                healthPool, statPool, modifierPool, unitPosTempPool, playerFilter
            );
            EventBus.Instance.Subscribe<CastSkillEvent>(OnCastSkillArg);
        }

        private void OnCastSkillArg(CastSkillEvent e)
        {
            if (skillSource.TryGetValue(e.SkillId, out var ability))
            {
                if (findTargets.TryGetValue(ability.AbilitySo.core.findTarget, out var findTarget))
                {
                    bool found = false;
                    int target = -1;
                    if (e.Team == Team.Player)
                    {
                        found = findTarget.Find(e.StartPosition, ability.AbilitySo.core.maxDistanceFindTarget,
                            monsterFilter, entity =>
                            {
                                int agentId = unitPool.Get(entity).agentId;
                                return shareData.Simulator.GetAgentPosition(agentId);
                            }, out target
                        );
                    }
                    else if (e.Team == Team.Monster)
                    {
                        found = findTarget.Find(e.StartPosition, ability.AbilitySo.core.maxDistanceFindTarget,
                            playerFilter, entity => float2.zero, out target
                        );
                    }
                    
                    if (found || !ability.AbilitySo.core.isRequireTarget)
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
                    Debug.LogError($"'{ability.AbilitySo.core.findTarget}' not registered to system, skill source '{e.SkillId}'");
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
            EcsPool<MonsterAgentData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool,
            EcsPool<HealthData> healthPool, EcsPool<StatData> statPool, EcsPool<UnitModifierData> modifierPool,
            EcsPool<MonsterTempData> unitPosTempPool,
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

                AbilitySO abilitySo = await KitLoaded.LoadAsync<AbilitySO>(path);
                dict[abilityId] = new AbilityLogic(abilitySo, skillData, 0, Team.Player, shareData, 
                    unitPool, shapePool, deadPool, healthPool, statPool, modifierPool, unitPosTempPool, playerFilter
                );

                if (abilitySo.core.bulletPrefab != null)
                {
                    KitPool.RegisterPool(abilitySo.core.bulletPrefab.gameObject, true);
                }
            }

            return dict;
        }

        private static string GetSkillAddressPath(int skillId) => skillId.ToString();
    }
}