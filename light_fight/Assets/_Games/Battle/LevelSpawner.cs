using System;
using System.Collections.Generic;
using System.Linq;
using _Games.Config;
using _Games.Utils;
using _KITSystem.ExcelConfig;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    [Serializable]
    public sealed class LevelSpawner : ITickable, IDisposable
    {
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        private Action<RequestCreateMonster> onCreateMonster;

        private IReadOnlyDictionary<WaveIdData, LevelBatch> container;
        private Batch[] batches;
        private int totalWave;
        private int currentWave = 1;
        private int currentBatch;
        private float waitTime;
        private float spawnTime;
        private bool paused = true;
        private bool waiting;
        
        public void Initialize(int levelId, Action<RequestCreateMonster> onCreateMonster)
        {
            this.onCreateMonster = onCreateMonster;
            monsterConfig = KitConfigManager.Get<MonsterConfig>();
            skillConfig = KitConfigManager.Get<SkillConfig>();
            
            LevelConfig levelConfig = KitConfigManager.Get<LevelConfig>();
            levelConfig.FindSpawn(levelId, out container);
            foreach (var pair in container)
            {
                totalWave = Mathf.Max(totalWave, pair.Key.WaveId);
            }
        }
        
        public void WaveSpawn()
        {
            LoadWave(currentWave);
            paused = false;
        }

        private void LoadWave(int waveIndex)
        {
            List<LevelBatch> values = new List<LevelBatch>();
            foreach (var pair in container)
            {
                if (pair.Key.WaveId == waveIndex) values.Add(pair.Value);
            }
            
            currentBatch = 0;
            batches = CreateBatches(values, monsterConfig, skillConfig);
            
            HashSet<int> monsters = new HashSet<int>();
            foreach (var batch in batches)
            {
                var keys = batch.monsters.Keys.ToList();
                foreach (var key in keys) monsters.Add(key);
            }
            
            foreach (var monster in monsters)
            {
                monsterConfig.Find(monster, out var monsterData);
                if (!monsterData.IsRanged) continue;
                skillConfig.Find(monsterData.SkillId, out var skillData);
            }

            waitTime = batches[0].waitTime;
            paused = false;
        }
        
        public void Tick(float dt)
        {
            if (paused) return;
            if (currentBatch < batches.Length)
            {
                waitTime -= dt;
                if (waitTime > 0) return;
                
                Batch batch = batches[currentBatch];
                spawnTime += dt;

                while (spawnTime > batch.interval)
                {
                    float2 position = RandomPointBetweenRects_NoLoop(
                        new float2(12, 22), new float2(14, 24), Vector2.zero
                    );
                    
                    int randomIndex = RandomUtils.Range(0, batch.monsters.Count);
                    int monsterID = batch.monsters.ElementAt(randomIndex).Key;
                    monsterConfig.Find(monsterID, out var monsterData);
                    onCreateMonster(new RequestCreateMonster(monsterID, monsterData.Radius, position));

                    batch.monsters[monsterID]--;
                    if (batch.monsters[monsterID] <= 0)
                        batch.monsters.Remove(monsterID);

                    spawnTime -= batch.interval;
                    batch.time -= batch.interval;
                }

                if (batch.monsters.Count == 0 || batch.time <= 0)
                {
                    currentBatch++;
                    if (currentBatch < batches.Length) waitTime = batches[currentBatch].waitTime;
                }

                if (currentBatch >= batches.Length)
                {
                    paused = true;
                    currentWave++;
                    waiting = true;
                }
            }

            if (waiting)
            {
                // Pause
            }
        }
        
        public void Dispose()
        {
            
        }
        
        private static Batch[] CreateBatches(List<LevelBatch> batches, MonsterConfig monsterConfig, SkillConfig skillConfig)
        {
            Batch[] result = new Batch[batches.Count];
            for (int i = 0; i < result.Length; i++)
            {
                LevelBatch batch = batches[i];
                Dictionary<int, int> monsters = new Dictionary<int, int>();
                List<int> bag = CreateRandomBag(batch.Weights);
                int bagIndex = 0;
                int powerBudget = batch.Power;
                int safe = 0;
                while (powerBudget > 0 && safe < 10)
                {
                    safe++;
                    if (bagIndex >= bag.Count)
                    {
                        CollectionUtils.Shuffle(ref bag);
                        bagIndex = 0;
                    }
                    
                    int enemyId = bag[bagIndex];
                    bagIndex++;

                    bool foundMonster = monsterConfig.Find(enemyId, out MonsterData monsterData);
#if DEBUG
                    if (!foundMonster) Debug.LogError("Not found monster: " + enemyId);
#endif
                    if (!foundMonster) break;

                    bool foundSkill = skillConfig.Find(monsterData.SkillId, out var skillData);
#if DEBUG
                    if (!foundSkill) Debug.LogError("Not found skill: " + monsterData.SkillId);
#endif
                    if (!foundSkill) break;
                    
                    int power = FormulaUtils.PowerMonster(monsterData, skillData, monsterData.SkillLevel);
                    if (power <= 0 || power > powerBudget)
                        continue;
                    powerBudget -= power;
                    if (monsters.TryGetValue(enemyId, out int count))
                        monsters[enemyId] = count + 1;
                    else monsters.Add(enemyId, 1);
                    safe = 0;
                }
                
                int total = 0;
                foreach (KeyValuePair<int, int> monster in monsters) 
                    total += monster.Value;
                result[i] = new Batch
                {
                    time = batch.Duration,
                    interval = batch.Duration / total,
                    monsters = monsters,
                    waitTime = batch.DelayTime
                };

#if UNITY_EDITOR
                if (monsters.Count == 0)
                {
                    Debug.LogError($"No monsters were spawned. PowerBudget: {powerBudget}");
                }
                else
                {
                    Debug.Log($"Build total {monsters.Count} monsters: " + string.Join(',', monsters) + $", duration: {batch.Duration}, interval: {batch.Duration / total}");
                }
#endif
            }

            return result;
        }
        
        private static List<int> CreateRandomBag(Vector2Int[] weights)
        {
            List<int> bag = new List<int>();

            foreach (var w in weights)
            {
                int id = w.x;
                int weight = w.y;

                for (int i = 0; i < weight; i++)
                {
                    bag.Add(id);
                }
            }

            CollectionUtils.Shuffle(ref bag);
            return bag;
        }
        
        static float2 RandomPointBetweenRects_NoLoop(float2 sizeA, float2 sizeB, float2 center)
        {
            float2 halfA = sizeA * 0.5f;
            float2 halfB = sizeB * 0.5f;

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
                return center + new float2(x, y);
            }

            r -= topArea;

            // BOTTOM
            if (r < bottomArea)
            {
                float x = RandomUtils.Range(-halfB.x, halfB.x);
                float y = RandomUtils.Range(-halfB.y, -halfA.y);
                return center + new float2(x, y);
            }

            r -= bottomArea;

            // LEFT
            if (r < leftArea)
            {
                float x = RandomUtils.Range(-halfB.x, -halfA.x);
                float y = RandomUtils.Range(-halfA.y, halfA.y);
                return center + new float2(x, y);
            }

            // RIGHT
            float xRight = RandomUtils.Range(halfA.x, halfB.x);
            float yRight = RandomUtils.Range(-halfA.y, halfA.y);
            return center + new float2(xRight, yRight);
        }
        
        private class Batch
        {
            public float time;
            public float waitTime;
            public float interval;
            public Dictionary<int, int> monsters;
        }
    }
}