using System.Collections.Generic;
using _Game.AbilitySystem;
using _Game.Battle.Data;
using _Game.Battle.Events;
using _KIT.Event;
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
            var modifierPool = world.GetPool<UnitModifierData>();
            var unitPosTempPool = world.GetPool<UnitPosTempData>();
            
            findTargets = new Dictionary<FindTargetType, IFindTarget>();
            findTargets.Add(FindTargetType.Farthest, new FarthestFindTarget(shareData.Simulator, unitPool));
            findTargets.Add(FindTargetType.Nearest, new NearestFindTarget(shareData.Simulator, unitPool));
            skillSource = await BuildAbilities(new Dictionary<int, string>
                {
                    { 0, "AbilityData" },
                    { 1, "AbilityData_1" },
                }, shareData, runtimeData, unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool,
                playerFilter);
            EventBus.Instance.Subscribe<CastSkillEvent>(OnCastSkillArg);
        }
        private static async UniTask<Dictionary<int, AbilityLogic>> BuildAbilities(
            Dictionary<int, string> abilitiesPath, BattleStartupShareData shareData, BattleStartupRuntimeData runtimeData,
            EcsPool<UnitData> unitPool, EcsPool<ShapeData> shapePool, EcsPool<DeadFlag> deadPool,
            EcsPool<HealthData> healthPool, EcsPool<UnitModifierData> modifierPool, EcsPool<UnitPosTempData> unitPosTempPool,
            EcsFilter playerFilter)
        {
            Dictionary<int, AbilityLogic> dict = new Dictionary<int, AbilityLogic>();
            foreach (var (abilityId, path) in abilitiesPath)
            {
                AbilityData abilityData = await KitLoaded.LoadAsync<AbilityData>(path);
                dict[abilityId] = new AbilityLogic(abilityData, Team.Player, shareData, runtimeData, 
                    unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool, playerFilter
                );
            }
            return dict;
        }

        private void OnCastSkillArg(CastSkillEvent e)
        {
            if (skillSource.TryGetValue(e.SkillId, out var ability))
            {
                if (findTargets.TryGetValue(ability.Data.core.findTarget, out var findTarget))
                {
                    EcsFilter filter = e.Team == Team.Player ? monsterFilter : playerFilter;
                    if (findTarget.Find(e.StartPosition, filter, out int target))
                    {
                        AbilityLogic abilityInstance = ability.CreateInstance(e.Team);
                        abilityInstance.Startup(e.Source, e.StartPosition, target);
                        additions.Enqueue(abilityInstance);
                    }
                    else
                    {
#if DEVELOP_MODE
                        Debug.LogError($"Không tìm thấy mục tiêu, kĩ năng id '{e.SkillId}'");
#endif
                    }
                }
                else
                {
#if DEVELOP_MODE
                    Debug.LogError($"Kiểu tìm mục tiêu '{ability.Data.core.findTarget}' chưa được đăng kí, kĩ năng id '{e.SkillId}'");   
#endif
                }
            }
            else
            {
#if DEVELOP_MODE
                Debug.LogError($"Kĩ năng id '{e.SkillId}' chưa được đăng kí");
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
                if(ability.IsCompleted) completes.Enqueue(ability);
            }

            while (completes.Count > 0)
            {
                var item = completes.Dequeue();
                item.Shutdown();
                item.Dispose();
                world.RemoveEventListener(item);
                CollectionUtils.RemoveFast(abilities, item);
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            EventBus.Instance.Unsubscribe<CastSkillEvent>(OnCastSkillArg);
        }
    }
}