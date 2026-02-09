
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

        private EcsFilter playerEcsFilter;

        private float elapsed;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            playerEcsFilter = world.Filter<UnitData>()
                .Inc<PlayerFlag>()
                .Exc<DeadFlag>()
                .End();
        }

        public void Run(IEcsSystems systems)
        {
            elapsed += shareData.TimeDelta;
            if (elapsed >= 0.2f)
            {
                elapsed = 0;
                
                foreach (var e in playerEcsFilter)
                {
                    EventBus.Instance.Publish(
                        new CastSkillEvent(e, 0, float2.zero, Team.Player)
                    );
                }
            }
        }
    }
}