using _Game.Battle.Systems;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Geometry;
using RVO;
using UnityEngine;
using _Game.Battle.Data;
using Unity.Mathematics;
#if UNITY_EDITOR
using _KIT.Utils;
using Leopotam.EcsLite.UnityEditor;
#endif

namespace _Game.Battle
{
    [RequireComponent(typeof(GameLoop))]
    public class BattleStartup : MonoBehaviour
    {
        public float2[] points;
        public float angle;
        
        private EcsWorld world;
        private EcsSystems systems;
        private GameLoop gameLoop;

        private float elapsed;
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            // todo: game loop
            gameLoop = GetComponent<GameLoop>();
            gameLoop.Pause();
            
            // todo: battle world
            world = new EcsWorld();
            BattleStartupShareData shareData = new BattleStartupShareData(
                new Simulator(), gameLoop.FrameDeltaTime
            );
            BattleStartupRuntimeData runtimeData = new BattleStartupRuntimeData();
            
            // todo: battle systems
            BattleEcsSystems ecsSystems = new BattleEcsSystems(world);
            
            gameLoop.Register(ecsSystems);
            
            systems = ecsSystems;
            systems
#if UNITY_EDITOR
                .Add(new EcsSystemsDebugSystem())
                .Add(new EcsWorldDebugSystem())
                .Add(new DrawSystem())
#endif
                .Add(new SpawnMonsterSystem())
                .Add(new MonsterMoveSystem())
                .Add(new Systems.AbilitySystem())
                .Add(new UnitCleanupSystem());

            systems.InjectShared(shareData);
            systems.InjectShared(runtimeData);
            systems.InitShared();
            systems.Init();
            
            Startup();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Polygon polygon = new Polygon(float2.zero, points, Mathf.Deg2Rad * angle);
            GeometryGizmos.DrawPolygon(polygon, Color.green, Time.deltaTime);

            if (world == null) return;
            
            elapsed += Time.deltaTime;
            if (elapsed >= 0.3f)
            {
                elapsed = 0;
                angle = RandomUtils.Range(0, 360);
            }
            else
            {
                return;
            }
            
            var unitPool = world.GetPool<UnitData>();
            var filter = world.Filter<UnitData>()
                .End();
            foreach (var entity in filter)
            {
                ref var unit = ref unitPool.Get(entity);
                if (Shape.TryGet(unit.shapeId, out var shape))
                {
                    if (shape.Type == ShapeType.Circle)
                    {
                        Circle c1 = new Circle(shape.CurrentPosition, shape.Radius);
                        if (GeometryPolygon.Overlaps(polygon, c1))
                        {
                            unit.color = Color.red;
                        }
                    }
                }
            }
        }
#endif

#if UNITY_EDITOR
        private void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                EventBus.Instance.Publish(new CastSkillEvent(0, 0, float2.zero, 0));
            }*/
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var dealPool = world.GetPool<DeadFlag>();
                var unitPool = world.GetPool<UnitData>();
                var filter = world.Filter<UnitData>()
                    .End();
                foreach (var entity in filter)
                {
                    if (unitPool.Get(entity).color == Color.red)
                    {
                        dealPool.Add(entity);
                    }
                }
            }
        }
#endif

        public void Startup()
        {
            gameLoop.Resume();
        }

        public void Shutdown()
        {
            gameLoop.Pause();
        }

        private void OnDestroy()
        {
            if (gameLoop != null)
            {
                gameLoop.Pause();
            }

            if (systems != null)
            {
                systems.Destroy();
                systems = null;
            }

            if (world != null)
            {
                world.Destroy();
                world = null;
            }
        }
    }
}