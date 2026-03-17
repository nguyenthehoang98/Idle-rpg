using System;
using System.Collections.Generic;
using System.Text;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Events;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Games.Combat.Level
{
    // todo: chuyển thành ecs.
    public partial class SpawnLogic : IDisposable
    {
        private readonly ShareData shareData;
        private readonly MonsterConfig monsterConfig;
        private readonly SkillConfig skillConfig;
        
        private WaveContainer waveContainer;
        private int waveIndex;
        private int batchIndex;
        private int spawnedCount;
        private double spawnElapsed;
        private float batchElapsed;
        private float waitElapsed;
        private bool isPaused = true;
        private bool waitingNextWave;

        public SpawnLogic(ShareData shareData)
        {
            this.shareData = shareData;
            this.monsterConfig = KitConfigManager.Get<MonsterConfig>();
            this.skillConfig = KitConfigManager.Get<SkillConfig>();
            
            waveContainer = new WaveContainer
            {
                waves = Build(shareData.LevelSpawn.waves, monsterConfig, skillConfig)
            };
            isPaused = false;
        }

        public async void Update(float dt)
        {
            Wave[] waves = waveContainer.waves;
            if (!isPaused && waveIndex < waves.Length)
            {
                Wave wave = waves[waveIndex];
                if (batchIndex < wave.batches.Length)
                {
                    waitElapsed -= dt;
                    if (waitElapsed > 0)
                        return;

                    Batch batch = wave.batches[batchIndex];
                    batchElapsed += dt;
                    spawnElapsed += dt;

                    // todo: while elapsed interval
                    while (spawnElapsed > 0 && spawnedCount < batch.monsters.Length)
                    {
                        if (batch.monsters.Length > spawnedCount)
                        {
                            int monster = batch.monsters[spawnedCount];
                            if (monsterConfig.Find(monster, out MonsterData monsterData))
                            {
                                Vector3 position = RandomPointBetweenRects_NoLoop(
                                    new Vector2(12, 22), new Vector2(14, 24), Vector2.zero
                                );
                                await ECSFactory.BuildMonster(monster, shareData.RadiusBonus, position);
                                spawnedCount++;
                                spawnElapsed -= batch.interval;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (spawnedCount >= batch.monsters.Length || batchElapsed >= batch.duration)
                    {
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
                        waitingNextWave = true;
                    }
                }
            }

            if (waitingNextWave && shareData.TotalCurrentMonsterAlive == 0)
            {
                waitingNextWave = false;
                EventBus.Instance.Publish(new WaveCompleteEvent(waveIndex));
            } 
        }

        public void Dispose()
        {
            SkillFactory.UnloadAll();
            ECSFactory.UnloadAll();
        }

        static Vector2 RandomPointBetweenRects_NoLoop(Vector2 sizeA, Vector2 sizeB, Vector2 center)
        {
            Vector2 halfA = sizeA * 0.5f;
            Vector2 halfB = sizeB * 0.5f;

            float topArea = sizeB.x * (halfB.y - halfA.y);
            float bottomArea = topArea;
            float leftArea = (halfB.x - halfA.x) * sizeA.y;
            float rightArea = leftArea;

            float totalArea = topArea + bottomArea + leftArea + rightArea;

            float r = RandomUtils.Range(0f, totalArea);

            // TOP
            if (r < topArea)
            {
                float x = RandomUtils.Range(-halfB.x, halfB.x);
                float y = RandomUtils.Range(halfA.y, halfB.y);
                return center + new Vector2(x, y);
            }

            r -= topArea;

            // BOTTOM
            if (r < bottomArea)
            {
                float x = RandomUtils.Range(-halfB.x, halfB.x);
                float y = RandomUtils.Range(-halfB.y, -halfA.y);
                return center + new Vector2(x, y);
            }

            r -= bottomArea;

            // LEFT
            if (r < leftArea)
            {
                float x = RandomUtils.Range(-halfB.x, -halfA.x);
                float y = RandomUtils.Range(-halfA.y, halfA.y);
                return center + new Vector2(x, y);
            }

            // RIGHT
            float xRight = RandomUtils.Range(halfA.x, halfB.x);
            float yRight = RandomUtils.Range(-halfA.y, halfA.y);
            return center + new Vector2(xRight, yRight);
        }
        
        public static void ValidateSpawn(LevelSpawnSO spawnSo, MonsterConfig monsterConfig,
            SkillConfig skillConfig)
        {
            List<LevelSpawnSO.WaveSpawn> waveSpawns = spawnSo.waves;
            Wave[] waves = Build(waveSpawns, monsterConfig, skillConfig);
            LogPower(waveSpawns, waves, spawnSo.name, monsterConfig, skillConfig);
        }

        static Wave[] Build(List<LevelSpawnSO.WaveSpawn> waveSpawns, MonsterConfig monsterConfig,
            SkillConfig skillConfig)
        {
            Wave[] waves = new Wave[waveSpawns.Count];
            for (int w = 0; w < waveSpawns.Count; w++)
            {
                LevelSpawnSO.WaveSpawn waveSpawn = waveSpawns[w];
                Wave wave = new Wave();
                wave.batches = new Batch[waveSpawn.batches.Count];
                for (int b = 0; b < wave.batches.Length; b++)
                {
                    // todo: xử lý tính toán số lượng quái sinh ra ở đay
                    LevelSpawnSO.BatchSpawn batchSpawn = waveSpawn.batches[b];
                    Batch batch = new Batch();
                    batch.duration = batchSpawn.duration;
                    batch.waitTime = batchSpawn.waitTimeSpawn;
                    int totalWeight = 0;
                    List<int> monsters = new List<int>();
                    foreach (LevelSpawnSO.EnemySpawn enemy in batchSpawn.enemies)
                    {
                        totalWeight += enemy.weight;
                    }

                    int powerBudget = batchSpawn.power;
                    int safe = 0;
                    while (powerBudget > 0 && safe < 10)
                    {
                        safe++;
                        int rand = RandomUtils.Range(0, totalWeight);
                        int indexSelect = batchSpawn.enemies.Count - 1;

                        for (int i = 0; i < batchSpawn.enemies.Count; i++)
                        {
                            rand -= batchSpawn.enemies[i].weight;
                            if (rand < 0)
                            {
                                indexSelect = i;
                                break;
                            }
                        }

                        int enemyId = batchSpawn.enemies[indexSelect].id;
                        if (!monsterConfig.Find(enemyId, out MonsterData monsterData))
                        {
#if UNITY_EDITOR
                            Debug.LogError("Not found monster: " + enemyId);
#endif
                            break;
                        }

                        if (!skillConfig.Find(monsterData.SkillId, monsterData.SkillLevel, out var skillStatData))
                        {
#if UNITY_EDITOR
                            Debug.LogError("Not found skill stat: " + monsterData.SkillId + ", level: " + monsterData.SkillLevel);
#endif
                            break;
                        }
                        
                        int power = FormulaUtils.PowerMonster(monsterData, skillStatData);
                        if (power <= 0 || power > powerBudget)
                            continue;
                        powerBudget -= power;
                        monsters.Add(enemyId);
                        safe = 0;
                    }

#if DEVELOP_MODE || COMBAT_FULL_LOG
                    if (monsters.Count == 0)
                    {
                        Debug.LogError($"No monsters were spawned. Wave: {w + 1}, Batch: {b + 1}, PowerBudget: {powerBudget}");
                    }
                    else
                    {
                        Debug.Log($"Build total {monsters.Count} monsters");
                    }
#endif
                    batch.interval = batchSpawn.duration / monsters.Count;
                    batch.monsters = monsters.ToArray();
                    batch.total = monsters.Count;
                    wave.batches[b] = batch;
                }

                waves[w] = wave;
            }

            return waves;
        }

        static void LogPower(List<LevelSpawnSO.WaveSpawn> waveSpawns, Wave[] waves, string name,
            MonsterConfig monsterConfig, SkillConfig skillConfig)
        {
#if DEVELOP_MODE
            int totalPower = 0;
            foreach (LevelSpawnSO.WaveSpawn waveSpawn in waveSpawns)
            {
                totalPower += waveSpawn.power;
            }

            int power = 0;
            foreach (Wave wave in waves)
            foreach (Batch batch in wave.batches)
            foreach (int monsterId in batch.monsters)
                if (monsterConfig.Find(monsterId, out MonsterData monsterData) &&
                    skillConfig.Find(monsterData.SkillId, monsterData.SkillLevel, out var skillStatData))
                {
                    power += FormulaUtils.PowerMonster(monsterData, skillStatData);
                }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < waveSpawns.Count; i++)
            {
                LevelSpawnSO.WaveSpawn waveSpawn = waveSpawns[i];
                Wave wave = waves[i];

                int wavePower = 0;
                List<int> count = new List<int>();
                List<float> interval = new List<float>();
                foreach (Batch batch in wave.batches)
                {
                    count.Add(batch.monsters.Length);
                    interval.Add((float)Math.Round(batch.interval, 3));
                    foreach (int monsterId in batch.monsters)
                    {
                        if (monsterConfig.Find(monsterId, out MonsterData monsterData) && 
                            skillConfig.Find(monsterData.SkillId, monsterData.SkillLevel, out var skillStatData))
                        {
                            wavePower += FormulaUtils.PowerMonster(monsterData, skillStatData);
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
    }
    
    public partial class SpawnLogic
    {
        struct WaveContainer
        {
            public Wave[] waves;
        }

        struct Wave
        {
            public Batch[] batches;
        }

        struct Batch
        {
            public float duration;
            public float waitTime;
            public double interval;
            public int[] monsters;
            public int total;
        }
    }
}