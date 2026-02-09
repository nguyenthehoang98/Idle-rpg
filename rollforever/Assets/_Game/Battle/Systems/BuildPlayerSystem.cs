
using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace _Game.Battle.Systems
{
    public class BuildPlayerSystem : IEcsInitSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var unitPool = world.GetPool<UnitData>();
            var playerPool = world.GetPool<PlayerFlag>();

            int entity = world.NewEntity();
            int agentId = shareData.Simulator.AddAgent(float2.zero);
            unitPool.Add(entity) = new UnitData(agentId);
#if UNITY_EDITOR
            unitPool.Get(entity).color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
#endif  
            playerPool.Add(entity);

            float radius = 0.5f;
            shareData.Simulator.SetAgentRadius(agentId, radius);
            shareData.Simulator.PauseAgent(agentId, true);
        }
    }
}
