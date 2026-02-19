
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
        private EcsFilter playerFilter;
        private EcsFilter monsterFilter;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            playerFilter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .Exc<DeadFlag>()
                .End();
            monsterFilter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            attackCasterPool = world.GetPool<AttackCasterData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in playerFilter)
            {
                ref var attack = ref attackCasterPool.Get(e);
                attack.elapsed += shareData.TimeDelta;
                if (attack.elapsed >= attack.cooldown && monsterFilter.GetEntitiesCount() > 0)
                {
                    attack.elapsed = 0;
                    EventBus.Instance.Publish(
                        new CastSkillEvent(e, 20101, float2.zero, Team.Player)
                    );
                }
            }
        }
    }
}