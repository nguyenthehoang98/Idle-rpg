using _Game.Battle.Systems;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Geometry;
using RVO;
using UnityEngine;
using _Game.Battle.Events;
using _KIT.Event;
using Unity.Mathematics;
#if UNITY_EDITOR
using Leopotam.EcsLite.UnityEditor;
#endif

namespace _Game.Battle
{
    [RequireComponent(typeof(GameLoop))]
    public class BattleStartup : MonoBehaviour
    {
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
                new Simulator(), new Matrix(100, 120, 0.5f),  gameLoop.FrameDeltaTime
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
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && world.GetEntitiesCount() > 0)
            {
                int[] entities = null;
                int count = world.GetAllEntities(ref entities);
                if (count > 0)
                    EventBus.Instance.Publish(new CastSkillEvent(0, 0, float2.zero, entities[0]));
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