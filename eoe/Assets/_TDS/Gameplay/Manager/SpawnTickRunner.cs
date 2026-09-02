using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Share;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using _TDS.Gameplay.Data;
using _TDS.Gameplay.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _TDS.Gameplay.Manager
{
    public sealed class SpawnTickRunner : TickRunner
    {
        [SerializeField] private Transform[] portals;

        public event Action<int> OnSpawnCompleted;

        private Dictionary<int, GameObject> cachedMonster = new Dictionary<int, GameObject>();
        private HashSet<string> names = new HashSet<string>();
        private SpawnTimer[] temps;

        private Dictionary<int, List<SpawnConfigData>> cachedByLevel;
        private List<SpawnConfigData> currents;
        private MonsterConfig monsterConfig;
        private SpawnConfig spawnConfig;
        private int maximumWave;
        private int waveIndex;

        public bool IsPaused { get; set; }
        public bool IsCompleted { get; private set; }

        public void SetLevel(int level)
        {
            monsterConfig = ConfigManager.Get<MonsterConfig>();
            spawnConfig = ConfigManager.Get<SpawnConfig>();

            if (!spawnConfig.TryGetSpawn(level, out var list))
            {
                Debug.LogError($"[SpawnTickRunner] Could not find spawn config for {level}");
            }
            else
            {
                cachedByLevel = new Dictionary<int, List<SpawnConfigData>>();

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
        }

        public async UniTask Initialize()
        {
            GameObject go;

            foreach (var pair in cachedByLevel)
            {
                foreach (var spawn in pair.Value)
                {
                    maximumWave = Mathf.Max(maximumWave, spawn.definition.wave);

                    int monsterId = spawn.monsterId;

                    if (cachedMonster.ContainsKey(monsterId)) continue;

                    if (!monsterConfig.TryGetMonsterData(monsterId, out var monsterData))
                    {
                        Debug.LogError($"[SpawnTickRunner] Could not find monster '{monsterId}'");
                        continue;
                    }

                    go = await AssetLoader.GetAssetCached<GameObject>(monsterData.deathVfx);

                    if (names.Add(monsterData.deathVfx))
                    {
                        Pool.RegisterPool(go, true);
                    }

                    go = await AssetLoader.GetAssetCached<GameObject>(monsterData.prefabName);

                    cachedMonster.TryAdd(monsterId, go);

                    if (names.Add(monsterData.prefabName))
                    {
                        Pool.RegisterPool(go, true);
                    }

                    if (!string.IsNullOrEmpty(monsterData.deathAudioClip) && names.Add(monsterData.deathAudioClip))
                    {
                        await AssetLoader.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
                    }
                }
            }

            string background = "1.background";

            go = await AssetLoader.GetAsset<GameObject>(background);

            Debug.LogError($"Fixed background is '{background}'");

            Object.Instantiate(go).transform.position = Vector3.zero;

            LoadWave(1);

            await UniTask.CompletedTask;
        }

        private bool LoadWave(int wave)
        {
            if (wave >= 0 && wave <= maximumWave)
            {
                waveIndex = wave;

                currents = cachedByLevel[wave];

                temps = new SpawnTimer[currents.Count];

                for (int i = 0; i < currents.Count; i++)
                {
                    SpawnConfigData spawn = currents[i];

                    temps[i] = new SpawnTimer(spawn.spawnsTime[0], spawn.spawnsTime[1], spawn.total);
                }

                return true;
            }

            IsCompleted = true;

            return false;
        }

        public override void Tick(float deltaTime)
        {
            if (IsPaused || IsCompleted) return;

            bool isWaveCompleted = true;

            for (var i = 0; i < currents.Count; i++)
            {
                SpawnTimer data = temps[i];

                int count = data.Spawn(deltaTime);

                if (count > 0)
                {
                    for (int j = 0; j < count; j++) Spawn(currents[i]);
                }

                if (!data.IsFinished) isWaveCompleted = false;

                temps[i] = data;
            }

            if (isWaveCompleted)
            {
                IsPaused = true;

                int wave = waveIndex;

                waveIndex++;

                LoadWave(waveIndex);

                OnSpawnCompleted?.Invoke(wave);
            }
        }

        private void Spawn(SpawnConfigData data)
        {
            int portalIndex = data.portals[0];

            if (data.portals.Length > 1)
            {
                portalIndex = RandomUtils.Range(0, data.portals.Length);
            }

            Vector3 position = portals[portalIndex].position + new Vector3(
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius),
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius)
            );

            GameObject instance = Pool.Instantiate(cachedMonster[data.monsterId], position, false);

            monsterConfig.TryGetMonsterData(data.monsterId, out MonsterData monsterData);

            Monster monster = instance.GetComponent<Monster>();

            AgentManager.Create_Agent(monster, data.scale, monsterData);
        }

        public void Dispose()
        {
            foreach (var pair in cachedMonster)
            {
                Pool.UnRegisterPool(pair.Value);
            }

            cachedMonster = null;

            foreach (var name in names)
            {
                AssetLoader.UnCache(name);
            }

            names = null;
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