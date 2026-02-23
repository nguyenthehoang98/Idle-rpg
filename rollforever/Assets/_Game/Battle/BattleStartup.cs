using _Game.Battle.Checker;
using _Game.Battle.Configs;
using _Game.Battle.Model;
using _Game.Battle.Systems;
using _KIT.Config;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Leopotam.EcsLite.UnityEditor;
using RVO;
using UnityEngine;

namespace _Game.Battle
{
    [RequireComponent(typeof(GameLoop))]
    public class BattleStartup : MonoBehaviour
    {
        [SerializeField] private bool enableFullBattleLog = true;
        private EcsWorld world;
        private EcsSystems systems;
        private GameLoop gameLoop;
        private BattleStartupShareData shareData;
        private float elapsed;
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
#if UNITY_EDITOR && (DEVELOP_MODE || COMBAT_FULL_LOG)
            gameObject.AddComponent<CpuFrame>();
#endif
        }
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            string mode = "COMBAT_FULL_LOG";
            if (!enableFullBattleLog && DefineSymbolUtils.Has(mode))
            {
                DefineSymbolUtils.Remove(mode);
            }
            else if (enableFullBattleLog && !DefineSymbolUtils.Has(mode))
            {
                DefineSymbolUtils.Add(mode);
            }
#endif
        }

        private async void Start()
        {
            // todo: game loop
            gameLoop = GetComponent<GameLoop>();
            gameLoop.Pause();

            LevelSpawnConfig spawnConfig = await LevelSpawnConfig.LoadSpawn(1);

            // todo: battle world
            world = new EcsWorld();
            Matrix matrix = new Matrix(100, 120, 0.5f);
            shareData = new BattleStartupShareData(
                gameLoop,
                new Simulator(), matrix, spawnConfig
            );
            BattleStartupRuntimeData runtimeData = new BattleStartupRuntimeData();

            // todo: battle systems
            BattleEcsSystems ecsSystems = new BattleEcsSystems(world);

            gameLoop.Register(ecsSystems);

            systems = ecsSystems;
            systems
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

            PlayerCasterSystem playerCasterSystem = systems.GetSystem<PlayerCasterSystem>();
            playerCasterSystem.SetActivePlayer(false);
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
            
            shareData.Dispose();
        }
    }
}