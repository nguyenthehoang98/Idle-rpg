using _Game.AbilitySystem;
using _Game.Battle.Systems;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using RVO;
using UnityEngine;
using _Game.Battle.Events;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
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
        private BattleStartupShareData shareData;
        private float elapsed;
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private async void Start()
        {
            await KitConfigManager.Load(new[]
            {
                "MonsterConfig"
            });
            
            // todo: game loop
            gameLoop = GetComponent<GameLoop>();
            gameLoop.Pause();

            LevelSpawnConfig spawnConfig = await LevelSpawnConfig.LoadSpawn(1);
            
            // todo: battle world
            world = new EcsWorld();
            shareData = new BattleStartupShareData(
                gameLoop,
                new Simulator(), new Matrix(100, 120, 0.5f), spawnConfig,
                gameLoop.FrameDeltaTime
            );
            BattleStartupRuntimeData runtimeData = new BattleStartupRuntimeData();
            
            // todo: battle systems
            BattleEcsSystems ecsSystems = new BattleEcsSystems(world);
            
            gameLoop.Register(ecsSystems);
            
            systems = ecsSystems;
            systems
                .Add(new BuildPlayerSystem())
#if UNITY_EDITOR && DEVELOP_MODE
                .Add(new EcsSystemsDebugSystem())
                .Add(new EcsWorldDebugSystem())
                .Add(new DrawSystem())
#endif
                .Add(new SpawnMonsterSystem())
                .Add(new MonsterMoveSystem())
                .Add(new PlayerCasterSystem())
                .Add(new MonsterCasterSystem())
                .Add(new Systems.AbilitySystem())
                .Add(new UnitCleanupSystem());

            systems.InjectShared(shareData);
            systems.InjectShared(runtimeData);
            systems.InitShared();
            systems.Init();
            
            Startup();
        }

        public void Startup() => gameLoop.Resume();

        public void Shutdown() => gameLoop.Pause();

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
            
            shareData.Matrix.Dispose();
        }
    }
}