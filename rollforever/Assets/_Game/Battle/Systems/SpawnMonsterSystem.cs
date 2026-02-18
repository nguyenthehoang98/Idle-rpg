using System;
using System.Collections.Generic;
using System.Text;
using _Game.AbilitySystem;
using _Game.Battle.Data;
using _Game.Battle.View;
using _Game.Configs;
using _KIT.Config;
using _KIT.Resource;
using _KIT.Utils;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Collections;
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
        private EcsPool<StatData> statPool;
        private EcsPool<HealthData> healthPool;
        private EcsPool<UnitData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<UnitPosTempData> unitPosTempPool;
        private EcsPool<MonsterFlag> monsterFlagPool;
        private EcsPool<AttackCasterData> casterPool;

        private SkillConfig skillConfig;
        private MonsterConfig monsterConfig;
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
            /*unitSource = new Dictionary<int, UnitView>();
            GameObject go = await KitLoaded.LoadAsync<GameObject>("UnitView");
            unitSource[0] = go.GetComponent<UnitView>();*/

            world = systems.GetWorld();
            statPool = world.GetPool<StatData>();
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
            monsterConfig = KitConfigManager.Get<MonsterConfig>();
            skillConfig = KitConfigManager.Get<SkillConfig>();
            waves = Build(shareData.LevelSpawnConfig.waves, monsterConfig, skillConfig);

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
                        if (batch.monsters.Count > spawnedCount)
                        {
                            var tuple = batch.monsters[spawnedCount];
                            if (monsterConfig.Find(tuple.id, out var monsterData))
                            {
                                float radius = 0.5f;
                                SpawnEntity(monsterData, tuple.level, float2.zero, 2, radius);
                                spawnedCount++;
                                spawnElapsed -= batch.interval;
                            }
                        }
                        else
                        {
                            break;
                        }
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

        private void SpawnEntity(MonsterConfig.MonsterData monsterData, int level, float2 center, float rangeLimit,
            float radius)
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
            unitPosTempPool.Add(entity) = new UnitPosTempData { stopDistance = monsterData.AttackDistance };
            healthPool.Add(entity) = new HealthData((int)monsterData.Health(level));
            modifierPool.Add(entity) = new UnitModifierData(StatusEffect.None);
            casterPool.Add(entity) = new AttackCasterData { cooldown = 0.5f, skillId = monsterData.SkillId };
            statPool.Add(entity) = new StatData()
                .Insert(StatType.Attack, new Stat(monsterData.Attack(level)))
                .Insert(StatType.Defense, new Stat(monsterData.Defense(level)))
                .Insert(StatType.Health, new Stat(monsterData.Health(level)))
                .Insert(StatType.MoveSpeed, new Stat(monsterData.MoveSpeed))
                .Insert(StatType.SkillReduceCooldown, new Stat(0))
                .Insert(StatType.CriticalRate, new Stat(0))
                .Insert(StatType.CriticalDamage, new Stat(0));
            
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

        static bool RaycastToSquareBorder(float2 pos, float2 center, float halfSize, out float2 hitPoint)
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
            float tExit = math.cmin(tMax);

            // không hit
            if (tExit < 0 || tEnter > tExit)
                return false;

            // hit đầu tiên khi ray đi vào hình vuông
            float t = tEnter >= 0 ? tEnter : tExit;

            hitPoint = pos + dir * t;
            return true;
        }

        public static void ValidateSpawn(LevelSpawnConfig spawnConfig, MonsterConfig monsterConfig,
            SkillConfig skillConfig)
        {
            List<LevelSpawnConfig.WaveSpawn> waveSpawns = spawnConfig.waves;
            List<Wave> waves = Build(waveSpawns, monsterConfig, skillConfig);
            LogPower(waveSpawns, waves, spawnConfig.name, monsterConfig, skillConfig);
        }

        static List<Wave> Build(List<LevelSpawnConfig.WaveSpawn> waveSpawns, MonsterConfig monsterConfig,
            SkillConfig skillConfig)
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
                    batch.monsters = new List<(int id, int level)>();

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
                            if (monsterConfig.Find(enemy.id, out var monsterData) &&
                                skillConfig.Find(monsterData.SkillId, out var skillData))
                            {
                                int power = FormulaUtils.PowerMonster(monsterData, enemy.level, skillData);
                                if (rand < power)
                                {
                                    lastLevel = enemy.level;
                                    lastPower = power;
                                    selected = enemy;
                                    break;
                                }

                                rand -= power;
                            }
                        }

                        if (selected == null)
                            break;

                        powerBudget -= lastPower;
                        batch.monsters.Add((selected.id, lastLevel));
                    }

                    batch.interval = batchSpawn.duration / batch.monsters.Count;
                    wave.batches[i] = batch;
                }

                waves.Add(wave);
            }

            return waves;
        }

        private static void LogPower(List<LevelSpawnConfig.WaveSpawn> waveSpawns, List<Wave> waves, string name,
            MonsterConfig monsterConfig, SkillConfig skillConfig)
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
                        if (monsterConfig.Find(monster.id, out var monsterData) &&
                            skillConfig.Find(monsterData.SkillId, out var skillData))
                        {
                            power += FormulaUtils.PowerMonster(monsterData, monster.level, skillData);
                        }
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
                    foreach (var tuple in batch.monsters)
                    {
                        if (monsterConfig.Find(tuple.id, out var monsterData) &&
                            skillConfig.Find(monsterData.SkillId, out var skillData))
                        {
                            wavePower += FormulaUtils.PowerMonster(monsterData, tuple.level, skillData);
                        }
                    }
                }

                sb.AppendLine($"Wave #{i + 1}: ({waveSpawn.power}:{wavePower}). " +
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
            public List<(int id, int level)> monsters;
        }
    }
}