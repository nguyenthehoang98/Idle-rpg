using System.Collections.Generic;
using _Game.Battle.Data;
using _Game.Battle.View;
using _KIT.Resource;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Battle.Systems
{
    public class SpawnMonsterSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Dictionary<int, UnitView> unitSource;
        
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;
        
        private EcsWorld world;
        private EcsPool<Unit> unitPool;
        private float tick;
        
        public async void Init(IEcsSystems systems)
        {
            unitSource = new Dictionary<int, UnitView>();
            GameObject go = await KitLoaded.LoadAsync<GameObject>("UnitView");
            unitSource[0] = go.GetComponent<UnitView>();
            
            world = systems.GetWorld();
            unitPool = world.GetPool<Unit>();
                
            shareData.Simulator.SetTimeStep(shareData.TimeDelta);
            shareData.Simulator.SetAgentDefaults(1f, 10, 10f, 10f, 1.5f, 5f, float2.zero);
        }

        public void Run(IEcsSystems systems)
        {
            tick += shareData.TimeDelta;
            if (tick >= 1.0f)
            {
                shareData.Simulator.EnsureCompleted();
                for (var i = 0; i < 1; i++)
                {
                    var goal = float2.zero;
                    var pos = RandomPointOnCircle(float2.zero, Random.Range(20, 30));
                    var velocity = math.normalize(goal - pos);
                    var agentId = shareData.Simulator.AddAgent(pos);

                    var entity = world.NewEntity();
                    unitPool.Add(entity) = new Unit
                    {
                        agentId = agentId,
                    };

                    shareData.Simulator.SetAgentRadius(agentId, 0.5f);
                    shareData.Simulator.SetAgentGoal(agentId, goal);
                    shareData.Simulator.SetAgentPrefVelocity(agentId, velocity);
                }

                tick = 0;
            }
        }

        private static float2 RandomPointOnCircle(float2 center, float radius)
        {
            var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return center + new float2(
                       Mathf.Cos(angle),
                       Mathf.Sin(angle)
                   ) * radius;
        }
    }
}