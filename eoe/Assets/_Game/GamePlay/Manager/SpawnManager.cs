using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game._GamePlay2;
using _Game.Configs;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay.Manager
{
    [Serializable]
    public sealed class SpawnManager : ITickable
    {
        [SerializeField] private Transform[] portals;

        public event Action<int> OnWaveSpawnCompleted;

        public bool IsPaused { get; set; }
        public bool IsCompleted { get; private set; }

        private SpawnTimer[] temps;
        private Dictionary<int, GameObject> cachedMonster = new Dictionary<int, GameObject>();
        private HashSet<string> names = new HashSet<string>();
        private MonsterConfig monsterConfig;
        private WaveData currentWaveData;
        private LevelData levelData;
        private int waveIndex;

        public void SetLevel(int level)
        {
            monsterConfig = ConfigManager.Get<MonsterConfig>();
            
            LevelConfig levelConfig = ConfigManager.Get<LevelConfig>();
            
            levelConfig.TryGetLevelData(level, out levelData);
        }

        public async Task Initialize()
        {
            GameObject go;

            for (int wave = 0; wave < levelData.waves.Length; wave++)
            {
                WaveData waveData = levelData.waves[wave];

                for (int spawn = 0; spawn < waveData.spawns.Length; spawn++)
                {
                    SpawnData spawnData = waveData.spawns[spawn];

                    int monsterId = spawnData.monsterId;

                    if (cachedMonster.ContainsKey(monsterId)) continue;

                    bool found = monsterConfig.TryGetMonsterData(monsterId, out var monsterData);
                    if (!found)
                    {
                        Debug.LogError($"Not found monster data with id '{monsterId}'");
                        continue;
                    }

                    go = await AssetBundleManager.GetAssetCached<GameObject>(monsterData.prefabName);

                    cachedMonster.TryAdd(monsterId, go);
                    
                    if (names.Add(monsterData.prefabName))
                    {
                        Pool.RegisterPool(go, true);
                    }
                }
            }

            go = await AssetBundleManager.GetAsset<GameObject>(levelData.backgroundPrefabName);
            Object.Instantiate(go).transform.position = Vector3.zero;

            LoadWave(0);

            await Task.CompletedTask;
        }

        private bool LoadWave(int wave)
        {
            if (wave >= 0 && wave < levelData.waves.Length)
            {
                waveIndex = wave;

                currentWaveData = levelData.waves[waveIndex];

                SpawnData[] spawnsData = currentWaveData.spawns;

                temps = new SpawnTimer[spawnsData.Length];

                for (int i = 0; i < spawnsData.Length; i++)
                {
                    SpawnData spawnData = spawnsData[i];

                    temps[i] = new SpawnTimer(spawnData.startTime, spawnData.endTime, spawnData.totalMonster);
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

            for (var i = 0; i < currentWaveData.spawns.Length; i++)
            {
                SpawnData spawnData = currentWaveData.spawns[i];

                SpawnTimer data = temps[i];

                int count = data.Spawn(deltaTime);

                if (count > 0)
                {
                    for (int j = 0; j < count; j++)
                    {
                        Spawn(spawnData);
                    }
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
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius),
                RandomUtils.Range(-data.spawnRadius, data.spawnRadius)
            );
            
            GameObject instance = Pool.Instantiate(cachedMonster[data.monsterId]);
            
            instance.transform.position = position;

            monsterConfig.TryGetMonsterData(data.monsterId, out MonsterData monsterData);
            
            Monster monster = instance.GetComponent<Monster>();
            
            MonsterRuntimeData runtimeData = new MonsterRuntimeData
            {
                AttackScale = data.monsterAttackScale,
                HealthScale = data.monsterHealthScale,
                ExpScale = data.monsterExpScale
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
    }
}