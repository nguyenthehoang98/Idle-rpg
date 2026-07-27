using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.Resource;
using _GameToolkit.Updater;
using _TDS.GameConfig;
using _TDS.Unit;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _TDS.Gameplay
{
    public class SpawnerUpdater : BaseUpdatable
    {
        [SerializeField] private Transform[] portals;

        public event Action<int> OnWaveSpawned; 

        private MonsterConfig _monsterConfig;
        private List<SpawnerConfigData> allSpawners;

        Dictionary<int, GameObject> enemyIdToGameObjects = new Dictionary<int, GameObject>();
        HashSet<string> assetsPath = new HashSet<string>();

        SpawnerConfigData[] currentData;
        SpawnTimer[] currentTimer;
        
        int maxWave;
        int currentWave;
        bool isPaused;

        public async UniTask Initialize(int level)
        {
            bool found;
            
            SpawnerConfig spawnerConfig = ConfigManager.Get<SpawnerConfig>();
            
            found = spawnerConfig.TryGetSpawner(level, out allSpawners);
            if (!found)
            {
                Debug.LogError($"Not found spawner at level '{level}'");
                return;
            }

            _monsterConfig = ConfigManager.Get<MonsterConfig>();

            for (int i = 0; i < allSpawners.Count; i++)
            {
                SpawnerConfigData spawner = allSpawners[i];

                maxWave = Mathf.Max(maxWave, spawner.wave);
                
                int monster = spawner.monster;

                if (enemyIdToGameObjects.ContainsKey(monster)) continue;

                found = _monsterConfig.TryGetMonster(monster, out MonsterConfigData monsterData);

                if (!found)
                {
                    Debug.LogError($"Not found monster at level '{monster}'");
                    
                    continue;
                }

                GameObject go = await AssetManager.GetAssetCached<GameObject>(monsterData.asset);
                
                enemyIdToGameObjects.TryAdd(monster, go);

                if (assetsPath.Add(monsterData.asset))
                {
                    Pool.RegisterPool(go, true);
                }
            }

            Debug.Log("Load background");
            
            LoadWaveIndex(1);

            isPaused = false;
        }
        
        public override void Tick(float deltaTime)
        {
            if (isPaused) return;
            
            if (currentData == null || currentTimer == null) return;
            
            bool isWaveCompleted = true;
            
            for (int i = 0; i < currentData.Length; i++)
            {
                SpawnerConfigData spawn = currentData[i];
                SpawnTimer timer = currentTimer[i];

                int count = timer.Spawn(deltaTime);

                if (count > 0)
                {
                    for (int ii = 0; ii < count; ii++) SpawnMonster(spawn);
                }

                if (!timer.IsFinished)
                {
                    isWaveCompleted = false;
                }

                currentTimer[i] = timer;
            }

            if (isWaveCompleted)
            {
                isPaused = true;
                
                OnWaveSpawned?.Invoke(currentWave);

                currentWave++;
                
                LoadWaveIndex(currentWave);
            }
        }

        private void LoadWaveIndex(int wave)
        {
            if (wave >= 0 && wave <= maxWave)
            {
                currentWave = wave;
                
                List<SpawnerConfigData> temp = new List<SpawnerConfigData>();

                foreach (var spawner in allSpawners)
                {
                    if(spawner.wave == wave) temp.Add(spawner);
                }
                
                currentData = temp.ToArray();

                currentTimer = new SpawnTimer[temp.Count];

                for (int i = 0; i < temp.Count; i++)
                {
                    SpawnerConfigData data = currentData[i];

                    currentTimer[i] = new SpawnTimer(
                        data.spawnStartTime, data.spawnEndTime, data.totalMonster
                    );
                }
            }
        }

        private void SpawnMonster(SpawnerConfigData configData)
        {
            int portal = configData.portals[0];

            if (configData.portals.Length > 1)
            {
                portal = Random.Range(0, configData.portals.Length);
            }

            Vector3 spawnPosition = portals[portal].position + new Vector3(
                Random.Range(-configData.spawnAreaRadius, configData.spawnAreaRadius),
                Random.Range(-configData.spawnAreaRadius, configData.spawnAreaRadius)
            );

            GameObject go = Pool.Instantiate(enemyIdToGameObjects[configData.monster], false);
            
            Monster monster = go.GetComponent<Monster>();

            if (monster == null)
            {
                Debug.LogError($"Not found Monster at Prefab '{go.name}'");
                
                return;
            }

            _monsterConfig.TryGetMonster(configData.monster, out MonsterConfigData enemyConfigData);

            MonsterRuntimeData runtimeData = new MonsterRuntimeData(
                configData.healthScale, configData.attackScale,
                configData.expScale, configData.sizeScale
            );
            
            monster.Initialize(spawnPosition, enemyConfigData, runtimeData);
        }

        private void OnDestroy()
        {
            foreach (var pair in enemyIdToGameObjects)
            {
                Pool.UnRegisterPool(pair.Value);
            }

            foreach (var assetPath in assetsPath)
            {
                AssetManager.UnCache(assetPath);
            }
        }
    }
}