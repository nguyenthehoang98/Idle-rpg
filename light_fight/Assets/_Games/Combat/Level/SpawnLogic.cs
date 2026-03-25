using System;
using System.Collections.Generic;
using System.Text;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Config;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Cysharp.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

        private Batch[] batches;
        private int currentWave;
        private int currentBatch;
        private float waitTime;
        private float elapsedTime;
        private float spawnTime;
        private int spawnCount;
        private bool paused = true;
        private bool waiting = false;
        private EntityQuery query;

        public SpawnLogic(ShareData shareData)
        {
            Debug.Log(@"Sẽ tính từng wave 1 để giảm việc cấp phát bộ nhớ ban đầu");
            this.shareData = shareData;
            this.monsterConfig = KitConfigManager.Get<MonsterConfig>();
            this.skillConfig = KitConfigManager.Get<SkillConfig>();
            
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            query = manager.CreateEntityQuery(typeof(MonsterTag));
            
            ReloadData(1);
        }

        private async void ReloadData(int wave)
        {
            List<LevelBatch> values = new List<LevelBatch>();
            foreach (var pair in shareData.LevelSpawn)
            {
                if (pair.Key.x == wave) values.Add(pair.Value);
            }

            currentBatch = 0;
            currentWave = wave;
            batches = CreateBatches(values, monsterConfig, skillConfig);
            
            HashSet<int> monsters = new HashSet<int>();
            foreach (var batch in batches)
            {
                foreach (var monster in batch.monsters)
                    monsters.Add(monster);
            }
            
            foreach (var monster in monsters)
            {
                monsterConfig.Find(monster, out var monsterData);
                await KitLoaded.LoadAsync<GameObject>(monsterData.MonsterObjectId, true);
                if (!monsterData.IsRanged) continue;
                skillConfig.Find(monsterData.SkillId, out var skillData);
                Skill skill = await SkillFactory.CreateSkill(skillData);
                KitPool.RegisterPool(skill.projectile.prefab, true);
            }

            waitTime = batches[0].waitTime;
            paused = false;
        }

        public async void Update(float dt)
        {
            if (paused) return;
            if (currentBatch < batches.Length)
            {
                waitTime -= dt;
                if (waitTime > 0) return;
                
                var batch = batches[currentBatch];
                elapsedTime += dt;
                spawnTime += dt;

                while (spawnTime > 0 && spawnCount < batch.monsters.Length)
                {
                    int monster = batch.monsters[spawnCount];
                    Vector3 position = RandomPointBetweenRects_NoLoop(
                        new Vector2(12, 22), new Vector2(14, 24), Vector2.zero
                    );
                    await ECSFactory.BuildMonster(monster, shareData.RadiusBonus, position);
                    spawnCount++;
                    spawnTime -= batch.interval;
                }

                if (spawnCount >= batch.monsters.Length || elapsedTime >= batch.duration)
                {
                    currentBatch++;
                    spawnCount = 0;
                    elapsedTime = waitTime = spawnTime = 0;
                    if (currentBatch < batches.Length) waitTime = batches[currentBatch].waitTime;
                }

                if (currentBatch >= batches.Length)
                {
                    paused = true;
                    currentWave++;
                    waiting = true;
                }
            }

            if (waiting && query.CalculateEntityCount() == 0)
            {
                waiting = false;
                EventBus.Instance.Publish(new WaveCompleteEvent(currentWave));
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

        private static Batch[] CreateBatches(List<LevelBatch> batches, MonsterConfig monsterConfig, SkillConfig skillConfig)
        {
            Batch[] result = new Batch[batches.Count];
            for (int i = 0; i < result.Length; i++)
            {
                LevelBatch batch = batches[i];
                List<int> monsters = new List<int>();
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
#if UNITY_EDITOR
                    if (!foundMonster) Debug.LogError("Not found monster: " + enemyId);
#endif
                    if (!foundMonster) break;

                    bool foundSkill = skillConfig.Find(monsterData.SkillId, out var skillData);
#if UNITY_EDITOR
                    if (!foundSkill) Debug.LogError("Not found skill: " + monsterData.SkillId);
#endif
                    if (!foundSkill) break;
                    
                    int power = FormulaUtils.PowerMonster(monsterData, skillData, monsterData.SkillLevel);
                    if (power <= 0 || power > powerBudget)
                        continue;
                    powerBudget -= power;
                    monsters.Add(enemyId);
                    safe = 0;
                }
                
                
#if DEVELOP_MODE || COMBAT_FULL_LOG
                if (monsters.Count == 0)
                {
                    Debug.LogError($"No monsters were spawned. PowerBudget: {powerBudget}");
                }
                else
                {
                    Debug.Log($"Build total {monsters.Count} monsters: " + string.Join(',', monsters));
                }
#endif
                result[i] = new Batch
                {
                    duration = batch.Duration,
                    interval = batch.Duration / monsters.Count,
                    monsters = monsters.ToArray(),
                    waitTime = batch.DelayTime
                };
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
    }
    
    public partial class SpawnLogic
    {
        struct Batch
        {
            public float duration;
            public float waitTime;
            public float interval;
            public int[] monsters;
        }
    }
}