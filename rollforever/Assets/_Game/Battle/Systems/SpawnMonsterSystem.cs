using System.Collections.Generic;
using _Game.Battle.Data;
using _Game.Battle.View;
using _KIT.Pool;
using _KIT.Resource;
using Geometry;
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
        private EcsPool<UnitPos> unitPosPool;
        private EcsPool<Unit> unitPool;
        private float tick;
        
        public async void Init(IEcsSystems systems)
        {
            unitSource = new Dictionary<int, UnitView>();
            GameObject go = await KitLoaded.LoadAsync<GameObject>("UnitView");
            unitSource[0] = go.GetComponent<UnitView>();
            
            world = systems.GetWorld();
            unitPool = world.GetPool<Unit>();
            unitPosPool = world.GetPool<UnitPos>();
                
            shareData.Simulator.SetTimeStep(shareData.TimeDelta);
            shareData.Simulator.SetAgentDefaults(7f, 10, 10f, 10f, 1.5f, 10f, new float2(0f, 0f));
        }

        public void Run(IEcsSystems systems)
        {
            tick += shareData.TimeDelta;
            if (tick >= 1.0f)
            {
                for (var i = 0; i < 1; i++)
                {
                    var goal = float2.zero;
                    var pos = RandomPointOnCircle(float2.zero, Random.Range(20, 30));
                    var velocity = math.normalize(goal - pos);
                    var agentId = shareData.Simulator.AddAgent(pos);
                    shareData.Simulator.SetAgentPrefVelocity(agentId, velocity);

                    ShapeInstance shape = ShapeInstance.Insert(ShapeType.Circle, Random.value);
                    
                    var entity = world.NewEntity();
                    unitPosPool.Add(entity) = new UnitPos
                    {
                        goal = goal, 
                    };
                    unitPool.Add(entity) = new Unit
                    {
                        agentId = agentId,
                        shapeId = shape.Id,
                        cellId = shareData.Grid.NewCellId()
                    };

                    shareData.Grid.InsertOrUpdate(new GridObject(agentId, pos, new float2(1,1) * shape.Radius * 0.5f));
                    shareData.Simulator.SetAgentRadius(agentId, shape.Radius);

                    GameObject instance = KitPool.Instantiate(unitSource[0].gameObject);
                    instance.transform.position = new Vector3(pos.x, pos.y);
                    UnitView view = instance.GetComponent<UnitView>();
                    view.Init(shareData);
                    runtimeData.Insert(entity, view);
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