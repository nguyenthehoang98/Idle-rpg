
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

        private EcsPool<UnitData> unitPool;
        private EcsFilter playerEcsFilter;
        private EcsFilter monsterEcsFilter;

        private float elapsed;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
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
            elapsed += shareData.TimeDelta;
            if (elapsed < 0.2f)
                return;

            elapsed = 0.0f;
            
            foreach (var e in playerEcsFilter)
            {
                var monsters = monsterEcsFilter.GetRawEntities();
                if (monsters.Length > 0)
                {
                    EventBus.Instance.Publish(
                        new CastSkillEvent(e, 0, float2.zero, monsters[0])
                    );
                }
            }
        }
    }
}