using _Game.Battle.Data;
using _Game.Battle.Events;
using _KIT.Event;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class MonsterCasterSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsPool<UnitData> unitPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsPool<AttackCasterData> attackCasterPool;
        private EcsFilter filter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            filter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Inc<AttackCasterData>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
            attackCasterPool = world.GetPool<AttackCasterData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in filter)
            {
                var unitPos = unitPosTempPool.Get(e);
                if (!unitPos.isStopped)
                    continue;
                
                ref var attack = ref attackCasterPool.Get(e);
                attack.elapsed += shareData.TimeDelta;
                if (attack.elapsed >= attack.cooldown)
                {
                    attack.elapsed = 0;
                    /*EventBus.Instance.Publish(
                        new CastSkillEvent(e, 1, shareData.Simulator.GetAgentPosition(unitPool.Get(e).agentId), Team.Monster)
                    );*/
                }
            }
        }
    }
}