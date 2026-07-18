using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Model;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay.Manager
{
    [Serializable]
    public sealed class SpawnManager : ITickable
    {
        [SerializeField] private Transform[] portals;

        public event Action<int> OnWaveSpawnCompleted;
      
        private Dictionary<int, GameObject> cachedMonster = new Dictionary<int, GameObject>();
        private HashSet<string> names = new HashSet<string>();
        private SpawnTimer[] temps;
       
        private MonsterConfig monsterConfig;
       
        private SpawnData[] currentSpawnsData;
        private LevelData levelData;

        private int maxWave;
        private int waveIndex;

        public bool IsPaused { get; set; }
        public bool IsCompleted { get; private set; }

        public void SetLevel(int level)
        {
            monsterConfig = ConfigManager.Get<MonsterConfig>();
            
            LevelConfig levelConfig = ConfigManager.Get<LevelConfig>();
            
            bool found = levelConfig.TryGetLevelData(level, out levelData);
            if (!found) Debug.LogError($"Level {level} not found");
        }

        public async Task Initialize()
        {
            GameObject go;

            for (int i = 0; i < levelData.spawns.Length; i++)
            {
                SpawnData spawnData = levelData.spawns[i];

                maxWave = Mathf.Max(maxWave, spawnData.wave);

                int monsterId = spawnData.monster;

                if (cachedMonster.ContainsKey(monsterId)) continue;

                bool found = monsterConfig.TryGetMonsterData(monsterId, out var monsterData);
                if (!found)
                {
                    Debug.LogError($"Not found monster data with id '{monsterId}'");
                    continue;
                }

                // CACHE VFX
                go = await AssetBundleManager.GetAssetCached<GameObject>(monsterData.deathVfx);
                    
                if (names.Add(monsterData.deathVfx))
                {
                    Pool.RegisterPool(go, true);
                }

                go = await AssetBundleManager.GetAssetCached<GameObject>(monsterData.prefabName);

                cachedMonster.TryAdd(monsterId, go);
                    
                if (names.Add(monsterData.prefabName))
                {
                    Pool.RegisterPool(go, true);
                }
                    
                if (!string.IsNullOrEmpty(monsterData.deathAudioClip) && names.Add(monsterData.deathAudioClip))
                {
                    await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
                }
            }

            go = await AssetBundleManager.GetAsset<GameObject>(levelData.backgroundPrefabName);
            Object.Instantiate(go).transform.position = Vector3.zero;

            LoadWave(1);

            await Task.CompletedTask;
        }

        private bool LoadWave(int wave)
        {
            if (wave >= 0 && wave <= maxWave)
            {
                waveIndex = wave;

                List<SpawnData> list = new List<SpawnData>();

                foreach (var spawnData in levelData.spawns)
                {
                    if(spawnData.wave == wave) list.Add(spawnData);
                }

                currentSpawnsData = list.ToArray();

                SpawnData[] spawnsData = currentSpawnsData;

                temps = new SpawnTimer[spawnsData.Length];

                for (int i = 0; i < spawnsData.Length; i++)
                {
                    SpawnData spawnData = spawnsData[i];

                    temps[i] = new SpawnTimer(spawnData.startTime, spawnData.endTime, spawnData.total);
                }

                return true;
            }

            IsCompleted = true;

            return false;
        }

        public void Tick(float deltaTime)
        {
            if (IsPaused || IsCompleted) return;

            bool isWaveCompleted = true;

            for (var i = 0; i < currentSpawnsData.Length; i++)
            {
                SpawnData spawnData = currentSpawnsData[i];

                SpawnTimer data = temps[i];

                int count = data.Spawn(deltaTime);

                if (count > 0)
                {
                    for (int j = 0; j < count; j++) Spawn(spawnData);
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

                OnWaveSpawnCompleted?.Invoke(wave);
            }
        }

        private void Spawn(SpawnData data)
        {
            int portalIndex = data.portals[0];
            
            if (data.portals.Length > 1)
            {
                portalIndex = RandomUtils.Range(0, data.portals.Length);
            }

            Vector3 position = portals[portalIndex].position + new Vector3(
                RandomUtils.Range(-data.radius, data.radius),
                RandomUtils.Range(-data.radius, data.radius)
            );
            
            GameObject instance = Pool.Instantiate(cachedMonster[data.monster], position, false);
            
            monsterConfig.TryGetMonsterData(data.monster, out MonsterData monsterData);
            
            Monster monster = instance.GetComponent<Monster>();
            
            MonsterRuntimeData runtimeData = new MonsterRuntimeData
            {
                AttackScale = data.attackScale,
                HealthScale = data.healthScale,
                ExpScale = data.expScale,
                Scale = data.scale,
            };
            
            AgentManager.Create_Agent(monster, runtimeData, monsterData);
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
                AssetBundleManager.UnCache(name);
            }

            names = null;
        }
        
        [Serializable]
        private struct SpawnTimer
        {
            private int total;

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