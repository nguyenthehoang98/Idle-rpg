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
                    Debug.Log("monster-caster: " + e);
                    /*EventBus.Instance.Publish(
                        new CastSkillEvent(e, 0, float2.zero, Team.Player)
                    );*/
                }
            }
        }
    }
}