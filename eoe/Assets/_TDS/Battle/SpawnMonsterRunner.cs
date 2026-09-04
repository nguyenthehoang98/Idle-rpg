using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Share;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _TDS.Battle
{
    public class SpawnMonsterRunner : TickRunner
    {
        [SerializeField] private Transform[] portals;
       
        private readonly Dictionary<int, GameObject> cachedMonster = new Dictionary<int, GameObject>();

        private readonly Dictionary<int, List<SpawnConfigData>> cachedByLevel =  new Dictionary<int, List<SpawnConfigData>>();
       
        private readonly HashSet<string> paths = new HashSet<string>();
       
        private List<SpawnConfigData> currents;
        private SpawnTimer[] timers;

        private AgentMovementRunner agentMovementRunner;
        private SpawnConfig spawnConfig;
        private MonsterConfig monsterConfig;
        private int maximumWave;
        private int waveIndex;

        public event Action<int> OnSpawnCompleted;
        /// <summary>Đã spawn xong wave + giết hết quái -> wave cleared (waveIndex vừa xong).</summary>
        public event Action<int> OnWaveCleared;
        /// <summary>Diệt hết quái wave cuối -> thắng game.</summary>
        public event Action OnGameWin;
        public bool IsPaused { get; set; }
        /// <summary>Đã spawn hết toàn bộ wave config.</summary>
        public bool IsCompleted { get; private set; }

        public int CurrentWave => waveIndex;
        private bool waitingKillAll; // spawn xong wave, đang chờ diệt hết quái trên sân

        public override void Tick(float deltaTime)
        {
            // đang chờ diệt hết quái của wave vừa spawn xong (KHÔNG bị IsPaused chặn)
            if (waitingKillAll)
            {
                if (agentMovementRunner.AliveCount > 0) return;

                // hết quái trên sân -> wave cleared
                waitingKillAll = false;

                int clearedWave = waveIndex;
                OnWaveCleared?.Invoke(clearedWave);
                Debug.Log($"[SpawnMonsterRunner] Wave {clearedWave} cleared");

                if (waveIndex >= maximumWave)
                {
                    // diệt hết wave cuối -> win
                    IsCompleted = true;
                    OnGameWin?.Invoke();
                    Debug.Log($"[SpawnMonsterRunner] WIN GAME! All waves cleared (wave {clearedWave}/{maximumWave})");
                    return;
                }

                waveIndex++;
                LoadWaveConfigData(waveIndex);
                IsPaused = false; // resume spawn wave mới
                Debug.Log($"[SpawnMonsterRunner] Start wave {waveIndex}");
                return;
            }

            if (IsPaused || IsCompleted || currents == null || timers == null) return;

            // spawn xong wave (timer hết) -> chuyển sang chờ diệt hết quái
            if (AllTimersFinished())
            {
                waitingKillAll = true;
                IsPaused = true; // tạm pause spawn (chờ kill all)
                OnSpawnCompleted?.Invoke(waveIndex);
                return;
            }

            // đang spawn wave hiện tại
            for (var i = 0; i < currents.Count; i++)
            {
                SpawnTimer data = timers[i];

                int count = data.Spawn(deltaTime);

                if (count > 0)
                {
                    for (int j = 0; j < count; j++) SpawnMonster(currents[i]);
                }

                timers[i] = data;
            }
        }

        private bool AllTimersFinished()
        {
            if (timers == null) return true;
            for (int i = 0; i < timers.Length; i++)
            {
                if (!timers[i].IsFinished) return false;
            }
            return true;
        }

        public async UniTask LoadLevelAsync(AgentMovementRunner agent, int level)
        {
            agentMovementRunner = agent;
            
            monsterConfig = ConfigManager.Get<MonsterConfig>();
           
            spawnConfig = ConfigManager.Get<SpawnConfig>();

            if (!spawnConfig.TryGetSpawn(level, out var list))
            {
                Debug.LogError($"[SpawnTickRunner] Could not find spawn config for {level}");
            }
            else
            {
                foreach (var item in list)
                {
                    int wave = item.definition.wave;

                    if (cachedByLevel.TryGetValue(wave, out var cached))
                    {
                        cached.Add(item);
                    }
                    else
                    {
                        cachedByLevel.Add(wave, new List<SpawnConfigData> { item });
                    }
                }
            }

            GameObject go;
            foreach (var pair in cachedByLevel)
            {
                foreach (var spawn in pair.Value)
                {
                    maximumWave = Mathf.Max(maximumWave, spawn.definition.wave);

                    int monsterId = spawn.monsterId;

                    if (cachedMonster.ContainsKey(monsterId)) continue;

                    if (!monsterConfig.TryGetMonster(monsterId, out var monsterData))
                    {
                        Debug.LogError($"[SpawnTickRunner] Could not find monster '{monsterId}'");
                        continue;
                    }

                    go = await AssetLoader.GetAssetCached<GameObject>(monsterData.prefabName);

                    cachedMonster.TryAdd(monsterId, go);

                    if (paths.Add(monsterData.prefabName))
                    {
                        Pool.RegisterPool(go, true);
                    }
                }
            }

            string background = "1.background";
           
            Debug.Log($"Fixed background is '{background}'");

            go = await AssetLoader.GetAsset<GameObject>(background);
            
            Object.Instantiate(go).transform.position = Vector3.zero;
            
            LoadWaveConfigData(1);

            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var pair in cachedMonster)
            {
                Pool.UnRegisterPool(pair.Value);
            }

            foreach (var path in paths)
            {
                AssetLoader.UnCache(path);
            }
        }
        
        private void SpawnMonster(SpawnConfigData data)
        {
            Vector3 position = portals[data.RandomPortal].position + new Vector3(
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius),
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius)
            );

            GameObject instance = Pool.Instantiate(cachedMonster[data.monsterId], position, false);

            monsterConfig.TryGetMonster(data.monsterId, out MonsterConfigData monsterData);

            agentMovementRunner.Create_Agent(instance.GetComponent<Monster>(), data.scale, monsterData);
        }
        
        private void LoadWaveConfigData(int wave)
        {
            if (wave >= 0 && wave <= maximumWave)
            {
                waveIndex = wave;

                currents = cachedByLevel[wave];

                timers = new SpawnTimer[currents.Count];

                for (int i = 0; i < currents.Count; i++)
                {
                    SpawnConfigData spawn = currents[i];

                    timers[i] = new SpawnTimer(spawn.SpawnStartTime, spawn.SpawnEndTime, spawn.total);
                }

                return;
            }

            IsCompleted = true;
        }
        
        private struct SpawnTimer
        {
            private readonly int total;
            private readonly float startTime;
            private readonly float endTime;

            private int spawnedCount;
            private float elapsedTime;

            public SpawnTimer(float startTime, float endTime, int total)
            {
                this.startTime = startTime;
                this.endTime = endTime;
                this.total = total;
                spawnedCount = 0;
                elapsedTime = 0;
            }

            public int Spawn(float deltaTime)
            {
                elapsedTime += deltaTime;

                if (elapsedTime < startTime) return 0;

                float duration = endTime - startTime;

                if (duration <= 0 || total <= 0) return 0;

                float progress = Mathf.Clamp01((elapsedTime - startTime) / duration);

                int expectedCount = Mathf.FloorToInt(progress * total);

                int spawnCount = expectedCount - spawnedCount;

                spawnedCount = expectedCount;

                return spawnCount;
            }

            public bool IsFinished => spawnedCount >= total;
        }
    }
}
