using _Game.Battle.Data;
using _Game.Battle.Systems;
using _KIT.Schedule;
using Geometry;
using Leopotam.EcsLite;
using RVO;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using Leopotam.EcsLite.UnityEditor;
using UnityEditor;
#endif

namespace _Game.Battle
{
    [RequireComponent(typeof(GameLoop))]
    public class BattleStartup : MonoBehaviour
    {
        private EcsWorld world;
        private IEcsSystems systems;
        private EcsPool<UnitPos> unitPosPool;
        private EcsFilter unitFilter;
        private BattleStartupShareData shareData;
        private Grid<IShapeData> grid; 
        private Simulator simulator;
        private GameLoop gameLoop;

        private void Start()
        {
            // todo: game loop
            gameLoop = GetComponent<GameLoop>();
            gameLoop.Pause();
            
            // todo: battle world
            world = new EcsWorld();
            simulator = new Simulator();
            grid = new Grid<IShapeData>(new float2(20, 30), 1, 16);
            shareData = new BattleStartupShareData(
                simulator, grid, gameLoop.FrameDeltaTime
            );
            
            // todo: battle systems
            BattleEcsSystems ecsSystems = new BattleEcsSystems(world, shareData);
            unitPosPool = world.GetPool<UnitPos>();
            unitFilter = world.Filter<UnitPos>().End();
            
            gameLoop.Register(ecsSystems);
            
            systems = ecsSystems;
            systems
#if UNITY_EDITOR
                .Add(new EcsSystemsDebugSystem())
                .Add(new EcsWorldDebugSystem())
#endif
                // add other
                .Add(new SpawnMonsterSystem())
                .Add(new MonsterMoveSystem())
                .Init();
            
            Startup();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (simulator != null)
            {
                simulator.EnsureCompleted();
                
                foreach (var e in unitFilter)
                {
                    UnitPos unit = unitPosPool.Get(e);
                    Vector2 position = simulator.GetAgentPosition(unit.agentId);
                    Handles.DrawWireDisc(position, Vector3.forward, 1f);
                }
            }

            if (grid != null)
            {
                grid.Draw(Color.green, gameLoop.FrameDeltaTime);
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