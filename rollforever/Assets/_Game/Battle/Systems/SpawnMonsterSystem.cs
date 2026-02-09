using System.Collections.Generic;
using _Game.AbilitySystem;
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
        private EcsPool<HealthData> healthPool;
        private EcsPool<UnitData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsPool<MonsterFlag> monsterFlagPool;
        private float tick;
        
        public async void Init(IEcsSystems systems)
        {
            unitSource = new Dictionary<int, UnitView>();
            GameObject go = await KitLoaded.LoadAsync<GameObject>("UnitView");
            unitSource[0] = go.GetComponent<UnitView>();
            
            world = systems.GetWorld();
            unitPool = world.GetPool<UnitData>();
            shapePool = world.GetPool<ShapeData>();
            unitPosTempPool = world.GetPool<UnitPosTempData>();
            monsterFlagPool = world.GetPool<MonsterFlag>();
            healthPool = world.GetPool<HealthData>();
            modifierPool = world.GetPool<UnitModifierData>();
            
            shareData.Simulator.SetTimeStep(shareData.TimeDelta);
            shareData.Simulator.SetAgentDefaults(1f, 10, 20f, 20f, 1.5f, 5f, float2.zero);
        }

        public void Run(IEcsSystems systems)
        {
            tick += shareData.TimeDelta;
            if (tick >= 1.0f)
            {
                shareData.Simulator.EnsureCompleted();

                float halfSize = 2;
                float radius = Random.Range(0.5f, 1.5f);
                float2 center = float2.zero;

                for (var i = 0; i < 1; i++)
                {
                    float2 pos = RandomPointOnCircle(center, Random.Range(20, 30));
                   
                    float2 goal;
                    if (RaycastToSquareBorder(pos, center, halfSize, out var hitPoint))
                    {
                        goal = hitPoint;
                    }
                    else
                    {
                        goal = ProjectPointToSquareBorder(pos, halfSize);
                    }

                    int agentId = shareData.Simulator.AddAgent(pos);

                    int entity = world.NewEntity();
                    unitPool.Add(entity) = new UnitData(agentId);
#if UNITY_EDITOR
                    unitPool.Get(entity).color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
#endif

                    // todo: set agent
                    shareData.Simulator.SetAgentRadius(agentId, radius);
                    shareData.Simulator.SetAgentNeighborDist(agentId, radius * 3f);
                    shareData.Simulator.SetAgentGoal(agentId, goal);
                    shareData.Simulator.SetAgentPrefVelocity(agentId, math.normalize(goal - pos));
                    
                    // todo: add component
                    shapePool.Add(entity) = ShapeData.Circle(radius);
                    unitPosTempPool.Add(entity) = new UnitPosTempData();
                    healthPool.Add(entity) = new HealthData(100);
                    modifierPool.Add(entity) = new UnitModifierData(StatusEffect.None);

                    // todo: add flag
                    monsterFlagPool.Add(entity);
                }

                tick = 0;
            }
        }

        static float2 RandomPointOnCircle(float2 center, float radius)
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            return center + new float2(
                       Mathf.Cos(angle),
                       Mathf.Sin(angle)
                   ) * radius;
        }
        
        static float2 ProjectPointToSquareBorder(float2 pos, float halfSize)
        {
            float2 p = pos;

            float absX = math.abs(p.x);
            float absY = math.abs(p.y);

            if (absX > absY)
            {
                // chạm cạnh trái / phải
                p.x = math.sign(p.x) * halfSize;
                p.y = math.clamp(p.y, -halfSize, halfSize);
            }
            else
            {
                // chạm cạnh trên / dưới
                p.y = math.sign(p.y) * halfSize;
                p.x = math.clamp(p.x, -halfSize, halfSize);
            }

            return p;
        }
        
        static bool RaycastToSquareBorder(
            float2 pos,
            float2 center,
            float halfSize,
            out float2 hitPoint
        )
        {
            hitPoint = float2.zero;

            float2 dir = math.normalize(center - pos);

            float2 min = center - halfSize;
            float2 max = center + halfSize;

            float2 invDir = 1.0f / dir;

            float2 t1 = (min - pos) * invDir;
            float2 t2 = (max - pos) * invDir;

            float2 tMin = math.min(t1, t2);
            float2 tMax = math.max(t1, t2);

            float tEnter = math.cmax(tMin);
            float tExit  = math.cmin(tMax);

            // không hit
            if (tExit < 0 || tEnter > tExit)
                return false;

            // hit đầu tiên khi ray đi vào hình vuông
            float t = tEnter >= 0 ? tEnter : tExit;

            hitPoint = pos + dir * t;
            return true;
        }
    }
}