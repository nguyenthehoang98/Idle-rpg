
using _Game.Battle.Data;
using _Game.Battle.Events;
using _KIT.Event;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    public class PlayerCasterSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsPool<AttackCasterData> attackCasterPool;
        private EcsFilter filter;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            filter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .Exc<DeadFlag>()
                .End();
            attackCasterPool = world.GetPool<AttackCasterData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in filter)
            {
                ref var attack = ref attackCasterPool.Get(e);
                attack.elapsed += shareData.TimeDelta;
                if (attack.elapsed >= attack.cooldown)
                {
                    attack.elapsed = 0;
                    /*EventBus.Instance.Publish(
                        new CastSkillEvent(e, 0, float2.zero, Team.Player)
                    );*/
                }
            }
        }
    }
}