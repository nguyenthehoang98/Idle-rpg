using System;
using System.Collections.Generic;
using System.Text;
using _Game.AbilitySystem;
using _Game.Battle.Data;
using _Game.Battle.View;
using _KIT.Resource;
using _KIT.Utils;
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
        private EcsPool<AttackCasterData> casterPool;
        
        private List<Wave> waves = new List<Wave>();
        private int waveIndex;
        private int batchIndex;
        private int spawnedCount;
        private double spawnElapsed;
        private float batchElapsed;
        private float waitElapsed;
        private bool isPaused;

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
            casterPool = world.GetPool<AttackCasterData>();

            // todo: setup agents 
            shareData.Simulator.SetTimeStep(shareData.TimeDelta);
            shareData.Simulator.SetAgentDefaults(1f, 10, 20f, 20f, 1.5f, 5f, float2.zero);

            // todo: cache spawn data
            //HashSet<MonsterId> monsters = new HashSet<MonsterId>();
            var waveSpawns = shareData.LevelSpawnConfig.waves;
            waves = Build(waveSpawns);

            // todo: preload assets
        }
        
        public void Run(IEcsSystems systems)
        {
            float dt = shareData.TimeDelta;
            if (waveIndex < waves.Count && !isPaused)
            {
                var wave = waves[waveIndex];
                if (batchIndex < wave.batches.Length)
                {
                    waitElapsed -= dt;
                    if (waitElapsed > 0)
                        return;
                    
                    shareData.Simulator.EnsureCompleted();

                    var batch = wave.batches[batchIndex];
                    batchElapsed += dt;
                    spawnElapsed += dt;

                    // todo: while elapsed interval
                    while (spawnElapsed > 0 && spawnedCount < batch.monsters.Count)
                    {
                        SpawnEntity(float2.zero, 2, RandomUtils.Range(0.5f, 1.25f));
                        spawnedCount++;
                        spawnElapsed -= batch.interval;
                    }

                    if (spawnedCount >= batch.monsters.Count || batchElapsed >= batch.duration)
                    {
                        Debug.Log($"Spawn. Count:{spawnedCount}, Elapsed:{batchElapsed}, Time:{shareData.Time}");
                        batchIndex++;
                        spawnedCount = 0;
                        batchElapsed = 0;
                        spawnElapsed = 0;
                        waitElapsed = 0;
                        if (batchIndex < wave.batches.Length)
                        {
                            waitElapsed = wave.batches[batchIndex].waitTime;
                        }
                    }

                    if (batchIndex >= wave.batches.Length)
                    {
                        isPaused = true;
                        waveIndex++;
                    }
                }
            }
        }

        private void SpawnEntity(float2 center, float rangeLimit, float radius)
        {
            float2 pos = RandomPointOnCircle(center, Random.Range(20, 30));
                   
            float2 goal;
            if (RaycastToSquareBorder(pos, center, rangeLimit, out var hitPoint))
            {
                goal = hitPoint;
            }
            else
            {
                goal = ProjectPointToSquareBorder(pos, rangeLimit);
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
            casterPool.Add(entity) = new AttackCasterData {cooldown = 0.5f};

            // todo: add flag
            monsterFlagPool.Add(entity);
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

        public static void ValidateSpawn(LevelSpawnConfig config)
        {
            List<LevelSpawnConfig.WaveSpawn> waveSpawns = config.waves;
            List<Wave> waves = Build(waveSpawns);
            LogPower(waveSpawns, waves, config.name);
        }

        static List<Wave> Build(List<LevelSpawnConfig.WaveSpawn> waveSpawns)
        {
            List<Wave> waves = new List<Wave>();
            foreach (var waveSpawn in waveSpawns)
            {
                Wave wave = new Wave();
                wave.batches = new Batch[waveSpawn.batches.Count];
                for (int i = 0; i < wave.batches.Length; i++)
                {
                    // todo: xử lý tính toán số lượng quái sinh ra ở đay
                    LevelSpawnConfig.BatchSpawn batchSpawn = waveSpawn.batches[i];
                    Batch batch = new Batch();
                    batch.duration = batchSpawn.duration;
                    batch.waitTime = batchSpawn.waitTimeSpawn;
                    batch.monsters = new List<MonsterId>();

                    int totalWeight = 0;
                    foreach (var enemy in batchSpawn.enemies)
                    {
                        totalWeight += enemy.weight;
                    }

                    int powerBudget = batchSpawn.power;
                    while (powerBudget > 0)
                    {
                        int rand = RandomUtils.Range(0, totalWeight);
                        int lastPower = 0;
                        int lastLevel = 0;
                        LevelSpawnConfig.EnemySpawn selected = null;
                        foreach (var enemy in batchSpawn.enemies)
                        {
                            int level = RandomUtils.Range(enemy.enemyLevelRange.x, enemy.enemyLevelRange.y);
                            int power = BattleFormula.PowerMonster(enemy.enemyId, level);
                            if (rand < power)
                            {
                                lastLevel = level;
                                lastPower = power;
                                selected = enemy;
                                break;
                            }

                            rand -= power;
                        }

                        if (selected == null)
                            break;

                        powerBudget -= lastPower;

                        MonsterId monsterId = new MonsterId(selected.enemyId, lastLevel);
                        batch.monsters.Add(monsterId);
                    }

                    batch.interval = batchSpawn.duration / batch.monsters.Count;
                    wave.batches[i] = batch;
                }

                waves.Add(wave);
            }

            return waves;
        }
        
        private static void LogPower(List<LevelSpawnConfig.WaveSpawn> waveSpawns, List<Wave> waves, string name)
        {
#if DEVELOP_MODE
            int totalPower = 0;
            foreach (var waveSpawn in waveSpawns)
            {
                totalPower += waveSpawn.power;
            }
            
            int power = 0;
            foreach (var wave in waves)
            {
                foreach (var batch in wave.batches)
                {
                    foreach (var monster in batch.monsters)
                    {
                        power += BattleFormula.PowerMonster(monster.id, monster.level);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < waveSpawns.Count; i++)
            {
                var waveSpawn = waveSpawns[i];
                var wave = waves[i];

                int wavePower = 0;
                List<int> count = new List<int>();
                List<float> interval = new List<float>();
                foreach (var batch in wave.batches)
                {
                    count.Add(batch.monsters.Count);
                    interval.Add((float)Math.Round(batch.interval, 3));
                    foreach (var monster in batch.monsters)
                    {
                        wavePower += BattleFormula.PowerMonster(monster.id, monster.level);
                    }
                }

                sb.AppendLine($"Wave #{i+1}: ({waveSpawn.power}:{wavePower}). " +
                              $"Count: {string.Join(',', count)}. " +
                              $"Interval: {string.Join(',', interval)}");
            }

            float threshold = 0.3f;
            float error = math.abs(power / (float)totalPower) - 1;
            if (math.abs(error) < threshold)
                Debug.Log($"LevelSpawnConfig: {name}. [v]");
            else
            {
                Debug.LogError($"LevelSpawnConfig: {name}. Sai số lớn: ({totalPower}:{power}) \n{sb})");
            }
#endif
        }
        
        sealed class Wave
        {
            public Batch[] batches;
        }

        sealed class Batch
        {
            public float duration;
            public float waitTime;
            public double interval;
            public List<MonsterId> monsters;
        }
    }
}