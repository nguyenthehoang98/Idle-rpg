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

        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsPool<MonsterCasterData> monsterCasterPool;
        private EcsFilter filter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            filter = world.Filter<MonsterCasterData>()
                .Exc<DeadFlag>()
                .End();
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
                
                ref var caster = ref monsterCasterPool.Get(e);
                caster.elapsed += shareData.TimeDelta;
                if (caster.elapsed >= caster.cooldown)
                {
                    caster.elapsed = 0;
                    /*EventBus.Instance.Publish(
                        new CastSkillEvent(e, caster.skillId, shareData.Simulator.GetAgentPosition(unitPool.Get(e).agentId), Team.Monster)
                    );*/
                }
            }
        }
    }
}