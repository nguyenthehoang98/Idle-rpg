using System.Collections.Generic;
using _Game.AbilitySystem;
using _Game.Battle.Data;
using _Game.Battle.Events;
using _KIT.Event;
using _KIT.Resource;
using _KIT.Utils;
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
        private EcsPool<UnitData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<DeadFlag> deadPool;
        private EcsPool<HealthData> healthPool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        
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
            skillSource = new Dictionary<int, AbilityLogic>();

            world = systems.GetWorld();
            monsterFilter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            playerFilter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
            shapePool = world.GetPool<ShapeData>();
            deadPool = world.GetPool<DeadFlag>();
            healthPool = world.GetPool<HealthData>();
            modifierPool = world.GetPool<UnitModifierData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
            
            findTargets = new Dictionary<FindTargetType, IFindTarget>();
            findTargets.Add(FindTargetType.Farthest, new FarthestFindTarget(shareData.Simulator, unitPool));
            findTargets.Add(FindTargetType.Nearest, new NearestFindTarget(shareData.Simulator, unitPool));

            AbilityData abilityData = await KitLoaded.LoadAsync<AbilityData>("AbilityData");
            skillSource[0] = new AbilityLogic(abilityData, shareData, runtimeData,
                unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool
            );
            AbilityData abilityData1 = await KitLoaded.LoadAsync<AbilityData>("AbilityData_1");
            skillSource[1] = new AbilityLogic(abilityData1, shareData, runtimeData,
                unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool
            );
            EventBus.Instance.Subscribe<CastSkillEvent>(OnCastSkillArg);
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
                        AbilityLogic abilityInstance = ability.CreateInstance(
                            unitPool, shapePool, deadPool, healthPool, modifierPool, unitPosTempPool
                        );
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