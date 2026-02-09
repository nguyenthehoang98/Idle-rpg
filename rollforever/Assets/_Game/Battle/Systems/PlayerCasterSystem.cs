
using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;

namespace _Game.Battle.Systems
{
    public class PlayerCasterSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsPool<UnitData> unitPool;
        private EcsFilter playerEcsFilter;
        private EcsFilter monsterEcsFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            playerEcsFilter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .Exc<DeadFlag>()
                .End();
            monsterEcsFilter = world.Filter<UnitData>()
                .Inc<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var e in playerEcsFilter)
            {
                var unit = unitPool.Get(e);
            }
        }
    }
}