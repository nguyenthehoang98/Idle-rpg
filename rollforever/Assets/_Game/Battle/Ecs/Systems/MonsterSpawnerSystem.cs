using System;
using System.Collections.Generic;
using System.Text;
using _Game.Battle.AbilitySystem;
using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Model;
using _Game.Battle.Ecs.View;
using _Game.Battle.Level;
using _Game.Battle.Utils;
using _Game.Scripts.Configs;
using _Game.Scripts.Model;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using Geometry;
using Geometry.Primary;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Ray = Geometry.Primary.Ray;

namespace _Game.Battle.Ecs.Systems
{
    [Serializable]
    public class MonsterSpawnerSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostDestroySystem
    {
        private Dictionary<int, UnitView> sourcePrefab;

        [EcsInject] private readonly BattleStartupShareData shareData;

        private EcsWorld world;
        private EcsPool<StatData> statPool;
        private EcsPool<HealthData> healthPool;
        private EcsPool<MonsterAgentData> unitPool;
        private EcsPool<ShapeData> shapePool;
        private EcsPool<UnitModifierData> modifierPool;
        private EcsPool<MonsterTempData> unitPosTempPool;
        private EcsPool<MonsterFlag> monsterFlagPool;
        private EcsPool<MonsterCasterData> monsterCasterPool;
        private EcsFilter monsterAliveFilter;

        private SkillConfig skillConfig;
        private MonsterConfig monsterConfig;
        private WaveContainer waveContainer;
        private int waveIndex;
        private int batchIndex;
        private int spawnedCount;
        private double spawnElapsed;
        private float batchElapsed;
        private float waitElapsed;
        private bool isPaused = true;
        private bool waitingNextWave;

        public async void Init(IEcsSystems systems)
        {
            sourcePrefab = new Dictionary<int, UnitView>();
            GameObject go = await KitLoaded.LoadAsync<GameObject>("UnitView");
            sourcePrefab[0] = go.GetComponent<UnitView>();
            KitPool.RegisterPool(go, true);

            world = systems.GetWorld();
            statPool = world.GetPool<StatData>();
            unitPool = world.GetPool<MonsterAgentData>();
            shapePool = world.GetPool<ShapeData>();
            unitPosTempPool = world.GetPool<MonsterTempData>();
            monsterFlagPool = world.GetPool<MonsterFlag>();
            healthPool = world.GetPool<HealthData>();
            modifierPool = world.GetPool<UnitModifierData>();
            monsterCasterPool = world.GetPool<MonsterCasterData>();
            
            monsterAliveFilter = world.Filter<MonsterFlag>()
                .Exc<DeadFlag>()
                .End();

            // todo: cache spawn data
            //HashSet<MonsterId> monsters = new HashSet<MonsterId>();
            monsterConfig = KitConfigManager.Get<MonsterConfig>();
            skillConfig = KitConfigManager.Get<SkillConfig>();
            waveContainer = new WaveContainer
            {
                waves = Build(shareData.LevelSpawnSo.waves, monsterConfig, skillConfig)
            };
#if UNITY_EDITOR && (COMBAT_FULL_LOG || DEVELOP_MODE)
            Debug.Log("Spawn wave: " + JsonUtility.ToJson(waveContainer));
#endif
            // todo: preload assets
            isPaused = false;
            
            EventBus.Instance.Subscribe<WaveResumeEvent>(OnWaveResume);
            EventBus.Instance.Subscribe<EntityChangedStatEvent>(OnEntityChangedStat);
        }

        public void Run(IEcsSystems systems)
        {
            float dt = shareData.TimeDelta;
            Wave[] waves = waveContainer.waves;
            if (!isPaused && waveIndex < waves.Length)
            {
                Wave wave = waves[waveIndex];
                if (batchIndex < wave.batches.Length)
                {
                    waitElapsed -= dt;
                    if (waitElapsed > 0)
                        return;

                    shareData.Simulator.EnsureCompleted();

                    Batch batch = wave.batches[batchIndex];
                    batchElapsed += dt;
                    spawnElapsed += dt;

                    // todo: while elapsed interval
                    while (spawnElapsed > 0 && spawnedCount < batch.monsters.Length)
                    {
                        if (batch.monsters.Length > spawnedCount)
                        {
                            MonsterData tuple = batch.monsters[spawnedCount];
                            if (monsterConfig.Find(tuple.id, out MonsterConfig.MonsterData monsterData))
                            {
                                float radius = 0.5f;
                                SpawnEntity(monsterData, tuple.level, float2.zero, radius);
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

            if (waitingNextWave && monsterAliveFilter.GetEntitiesCount() == 0)
            {
                waitingNextWave = false;
                EventBus.Instance.Publish(new WaveCompleteEvent(waveIndex));
            }
        }

        public void PostDestroy(IEcsSystems systems)
        {
            EventBus.Instance.Subscribe<EntityChangedStatEvent>(OnEntityChangedStat);
            EventBus.Instance.Unsubscribe<WaveResumeEvent>(OnWaveResume);
        }

        private void OnEntityChangedStat(EntityChangedStatEvent e)
        {
            if (monsterFlagPool.Has(e.Entity))
            {
                var stat = statPool.Get(e.Entity);
                stat.TryGetValue(StatType.MoveSpeed, out var moveSpeed);
                
                var unit = unitPool.Get(e.Entity);
                shareData.Simulator.SetAgentMaxSpeed(unit.agentId, moveSpeed.Value);
            }
        }

        private void OnWaveResume(WaveResumeEvent e)
        {
            batchIndex = 0;
            spawnedCount = 0;
            batchElapsed = 0;
            spawnElapsed = 0;
            waitElapsed = 0;
            isPaused = false;
        }
        
        private void SpawnEntity(MonsterConfig.MonsterData monsterData, int level, float2 center, float radius)
        {
            float2 pos = RandomPointBetweenRects_NoLoop(new Vector2(10, 17), new Vector2(12, 19), center);

            float2 goal;
            Ray ray = new Ray(pos, math.normalize(center - pos));
            Shape shape = new Shape { type = ShapeType.Box, size = shareData.BoxSize };
            if (GeometryUtils.Ray(ray, 99, shape, float2.zero, out _, out var hitPoint))
            {
                goal = hitPoint;
            }
            else
            {
                goal = ProjectPointToSquareBorder(pos, shareData.BoxSize);
            }

            int agentId = shareData.Simulator.AddAgent(pos);
            int entity = world.NewEntity();
            unitPool.Add(entity) = new MonsterAgentData(agentId);
#if UNITY_EDITOR
            unitPool.Get(entity).color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
#endif

            // todo: set agent
            shareData.Simulator.SetAgentMaxSpeed(agentId, monsterData.MoveSpeed);
            shareData.Simulator.SetAgentRadius(agentId, radius);
            shareData.Simulator.SetAgentNeighborDist(agentId, radius * 3f);
            shareData.Simulator.SetAgentGoal(agentId, goal);
            shareData.Simulator.SetAgentPrefVelocity(agentId, math.normalize(goal - pos));

            // todo: add component
            shapePool.Add(entity) = ShapeData.Circle(radius);
            unitPosTempPool.Add(entity) = new MonsterTempData
            {
#if UNITY_EDITOR
                goal = goal,
#endif
                stopDistance = shareData.BoxSize + Vector2.one * monsterData.AttackDistance
            };
            healthPool.Add(entity) = new HealthData((int)monsterData.Health(level));
            modifierPool.Add(entity) = new UnitModifierData(StatusEffect.None);
            monsterCasterPool.Add(entity) = new MonsterCasterData
            {
                skillId = monsterData.SkillId,
                cooldown = monsterData.SkillCooldown,
            };
            statPool.Add(entity) = new StatData()
                .Insert(StatType.Attack, new Stat(monsterData.Attack(level)))
                .Insert(StatType.Defense, new Stat(monsterData.Defense(level)))
                .Insert(StatType.MaxHealth, new Stat(monsterData.Health(level)))
                .Insert(StatType.MoveSpeed, new Stat(monsterData.MoveSpeed))
                .Insert(StatType.CriticalRate, new Stat(0))
                .Insert(StatType.CriticalDamage, new Stat(0));

            // todo: add flag
            monsterFlagPool.Add(entity);
            
            // todo: build view
            UnitView view = KitPool.Instantiate(sourcePrefab[0]);
            view.Init(entity, pos, shareData);
        }

        #region Static
        
        static Vector2 RandomPointBetweenRects_NoLoop(Vector2 sizeA, Vector2 sizeB, Vector2 center)
        {
            Vector2 halfA = sizeA * 0.5f;
            Vector2 halfB = sizeB * 0.5f;

            float topArea = sizeB.x * (halfB.y - halfA.y);
            float bottomArea = topArea;
            float leftArea = (halfB.x - halfA.x) * sizeA.y;
            float rightArea = leftArea;

            float totalArea = topArea + bottomArea + leftArea + rightArea;

            float r = Random.Range(0f, totalArea);

            // TOP
            if (r < topArea)
            {
                float x = Random.Range(-halfB.x, halfB.x);
                float y = Random.Range(halfA.y, halfB.y);
                return center + new Vector2(x, y);
            }

            r -= topArea;

            // BOTTOM
            if (r < bottomArea)
            {
                float x = Random.Range(-halfB.x, halfB.x);
                float y = Random.Range(-halfB.y, -halfA.y);
                return center + new Vector2(x, y);
            }

            r -= bottomArea;

            // LEFT
            if (r < leftArea)
            {
                float x = Random.Range(-halfB.x, -halfA.x);
                float y = Random.Range(-halfA.y, halfA.y);
                return center + new Vector2(x, y);
            }

            // RIGHT
            float xRight = Random.Range(halfA.x, halfB.x);
            float yRight = Random.Range(-halfA.y, halfA.y);
            return center + new Vector2(xRight, yRight);
        }

        static float2 ProjectPointToSquareBorder(float2 pos, float2 size)
        {
            float2 p = pos;
            float2 halfSize = size / 2;

            float absX = math.abs(p.x);
            float absY = math.abs(p.y);

            if (absX > absY)
            {
                // chạm cạnh trái / phải
                p.x = math.sign(p.x) * halfSize.x;
                p.y = math.clamp(p.y, -halfSize.y, halfSize.y);
            }
            else
            {
                // chạm cạnh trên / dưới
                p.y = math.sign(p.y) * halfSize.y;
                p.x = math.clamp(p.x, -halfSize.x, halfSize.x);
            }

            return p;
        }

        static bool RaycastToSquareBorder(float2 pos, float2 center, float2 size, out float2 hitPoint)
        {
            hitPoint = float2.zero;
            float2 halfSize = size / 2;

            float2 dir = math.normalize(center - pos);

            float2 min = center - halfSize.x;
            float2 max = center + halfSize.y;

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
                    List<MonsterData> monsters = new List<MonsterData>();
                    int totalWeight = 0;
                    foreach (LevelSpawnSO.EnemySpawn enemy in batchSpawn.enemies)
                    {
                        totalWeight += enemy.weight;
                    }

                    int powerBudget = batchSpawn.power;
                    while (powerBudget > 0)
                    {
                        int rand = RandomUtils.Range(0, totalWeight);
                        int lastPower = 0;
                        int lastLevel = 0;
                        LevelSpawnSO.EnemySpawn selected = null;
                        foreach (LevelSpawnSO.EnemySpawn enemy in batchSpawn.enemies)
                        {
                            bool flag1 = monsterConfig.Find(enemy.id, out MonsterConfig.MonsterData monsterData);
#if DEVELOP_MODE || COMBAT_FULL_LOG
                            if (!flag1) Debug.LogError($"Not found enemy with id '{enemy.id}'");
#endif
                            bool flag2 = skillConfig.Find(monsterData.SkillId, out SkillConfig.SkillData skillData);
#if DEVELOP_MODE || COMBAT_FULL_LOG
                            if (!flag2) Debug.LogError($"Not found skill with skill_id '{skillData.SkillId}'");
#endif
                            if (flag1 && flag2)
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
                        monsters.Add(new MonsterData(selected.id, lastLevel));
                    }

#if DEVELOP_MODE || COMBAT_FULL_LOG
                    if (monsters.Count == 0)
                    {
                        Debug.LogError($"No monsters were spawned. Wave: {w + 1}, Batch: {b + 1}, PowerBudget: {powerBudget}");
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
            {
                foreach (Batch batch in wave.batches)
                {
                    foreach (MonsterData monster in batch.monsters)
                    {
                        if (monsterConfig.Find(monster.id, out MonsterConfig.MonsterData monsterData) &&
                            skillConfig.Find(monsterData.SkillId, out SkillConfig.SkillData skillData))
                        {
                            power += FormulaUtils.PowerMonster(monsterData, monster.level, skillData);
                        }
                    }
                }
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
                    foreach (MonsterData tuple in batch.monsters)
                    {
                        if (monsterConfig.Find(tuple.id, out MonsterConfig.MonsterData monsterData) &&
                            skillConfig.Find(monsterData.SkillId, out SkillConfig.SkillData skillData))
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

        #endregion

        #region Struct Data
        
        [Serializable]
        struct WaveContainer
        {
            public Wave[] waves;
        }

        [Serializable]
        struct Wave
        {
            public Batch[] batches;
        }

        [Serializable]
        struct Batch
        {
            public float duration;
            public float waitTime;
            public double interval;
            public MonsterData[] monsters { get; set; }
            public int total;
        }
        
        [Serializable]
        struct MonsterData
        {
            public int id;
            public int level;

            public MonsterData(int id, int level)
            {
                this.id = id;
                this.level = level;
            }
        }

        #endregion
    }
}