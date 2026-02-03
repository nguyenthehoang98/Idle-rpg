using _Game.Battle.Data;
using Geometry;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class SpawnMonsterSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        private EcsPool<UnitPos> unitPosPool;
        private BattleStartupShareData shareData;
        private float tick;
        
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            unitPosPool = world.GetPool<UnitPos>();
            shareData = systems.GetShared<BattleStartupShareData>();
            shareData.Simulator.SetTimeStep(shareData.TimeDelta);
            shareData.Simulator.SetAgentDefaults(7f, 10, 10f, 10f, 1.5f, 4f, new float2(0f, 0f));
        }

        public void Run(IEcsSystems systems)
        {
            tick += shareData.TimeDelta;
            if (tick >= 1.0f)
            {
                for (var i = 0; i < 1; i++)
                {
                    var goal = float2.zero;
                    var pos = RandomPointOnCircle(float2.zero, UnityEngine.Random.Range(20, 30));
                    var velocity = math.normalize(goal - pos);
                    var agentId = shareData.Simulator.AddAgent(pos);
                    shareData.Simulator.SetAgentPrefVelocity(agentId, velocity);

                    var entity = world.NewEntity();
                    unitPosPool.Add(entity) = new UnitPos
                    {
                        agentId = agentId, goal = goal, 
                    };
                    
                    shareData.Grid.InsertOrUpdate(new BoxData2D(agentId, pos, new float2(0.5f, 0.5f)));
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