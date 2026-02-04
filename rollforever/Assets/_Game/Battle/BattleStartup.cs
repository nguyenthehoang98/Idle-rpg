using System;
using _Game.Battle.Events;
using _Game.Battle.Systems;
using _KIT.Event;
using _KIT.Schedule;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using RVO;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using _Game.Battle.Data;
using Leopotam.EcsLite.UnityEditor;
#endif

namespace _Game.Battle
{
    [RequireComponent(typeof(GameLoop))]
    public class BattleStartup : MonoBehaviour
    {
        [SerializeField] private float circleRadius = 2f;
        [SerializeField] private float2 circleCenter;
        
        private EcsWorld world;
        private EcsSystems systems;
        private GameLoop gameLoop;

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
            UnityEditor.Handles.DrawWireDisc((Vector2) circleCenter, Vector3.forward, circleRadius);

            if (world == null) return;
            var unitPool = world.GetPool<UnitData>();
            var filter = world.Filter<UnitData>()
                .Inc<UnitPosData>()
                .End();
            foreach (var entity in filter)
            {
                var unit = unitPool.Get(entity);
                if (Shape.TryGet(unit.shapeId, out var shape))
                {
                    
                }
            }
        }
#endif

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EventBus.Instance.Publish(new CastSkillEvent(0, 0, float2.zero, 0));
            }
        }

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