using System.Linq;
using _Game.Battle.Systems;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using RVO;
using UnityEngine;
using _KIT.Config;
#if UNITY_EDITOR
using Leopotam.EcsLite.UnityEditor;
#endif

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
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
            gameObject.AddComponent<BattleDebugView>();
#endif

            await KitConfigManager.Load(new[]
            {
                "MonsterConfig",
                "SkillConfig",
                "WeaponConfig",
            });

            // todo: game loop
            gameLoop = GetComponent<GameLoop>();
            gameLoop.Pause();

            LevelSpawnConfig spawnConfig = await LevelSpawnConfig.LoadSpawn(1);

            // todo: battle world
            world = new EcsWorld();
            Matrix matrix = new Matrix(100, 120, 0.5f);
            shareData = new BattleStartupShareData(
                gameLoop,
                new Simulator(), matrix, spawnConfig,
                gameLoop.FrameDeltaTime
            );
            BattleStartupRuntimeData runtimeData = new BattleStartupRuntimeData();
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
            BattleDebugView debugView = gameObject.GetComponent<BattleDebugView>();
            debugView.InjectShareData(shareData);
            world.AddEventListener(debugView);
#endif

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

            PlayerCasterSystem playerCasterSystem =
                systems.GetAllSystems().FirstOrDefault(system => system.GetType() == typeof(PlayerCasterSystem))
                    as PlayerCasterSystem;
            for (int i = 0; i < 8; i++)
            {
                playerCasterSystem.Equip(i, 2001, 1);
            }

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