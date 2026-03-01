using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Model;
using _KIT.Event;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;

namespace _Game.Battle.Ecs.Systems
{
    public class MonsterCasterSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsPool<UnitData> unitPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsPool<MonsterCasterData> monsterCasterPool;
        private EcsFilter filter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            filter = world.Filter<MonsterCasterData>()
                .Inc<UnitData>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
            monsterCasterPool = world.GetPool<MonsterCasterData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in filter)
            {
                var unitPos = unitPosTempPool.Get(e);
                if (!unitPos.isStopped)
                    continue;

                int agentId = unitPool.Get(e).agentId;
                var agentPosition = shareData.Simulator.GetAgentPosition(agentId);
                
                ref var caster = ref monsterCasterPool.Get(e);
                caster.elapsed += shareData.TimeDelta;
                if (caster.elapsed >= caster.cooldown)
                {
                    caster.elapsed = 0;
                    EventBus.Instance.Publish(
                        new CastSkillEvent(e, caster.skillId, agentPosition, Team.Monster)
                    );
                }
            }
        }
    }
}