using _Game.Battle.Data;
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
            shareData.Simulator.SetAgentDefaults(7f, 10, 10f, 10f, 1.5f, 2f, new float2(0f, 0f));
        }

        public void Run(IEcsSystems systems)
        {
            tick += shareData.TimeDelta;
            if (tick >= 1.0f)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 position = RandomPointOnCircle(Vector2.zero, UnityEngine.Random.Range(10, 15));
                    Vector2 velocity = -position.normalized;
                    int agentId = shareData.Simulator.AddAgent(position);
                    shareData.Simulator.SetAgentVelocity(agentId, velocity);

                    int entity = world.NewEntity();
                    unitPosPool.Add(entity) = new UnitPos {agentId = agentId, goal = float2.zero};
                }
            }
        }

        private static Vector2 RandomPointOnCircle(Vector2 center, float radius)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return center + new Vector2(
                       Mathf.Cos(angle),
                       Mathf.Sin(angle)
                   ) * radius;
        }
    }
}