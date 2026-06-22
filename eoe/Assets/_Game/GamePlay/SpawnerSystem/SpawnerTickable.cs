using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Configs;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay.SpawnerSystem
{
    [Serializable]
    public class SpawnerTickable : ITickable
    {
        public event Action<int> OnWaveSpawnCompleted;

        public Transform[] portals;

        public bool IsPaused { get; set; }
        public bool IsCompleted { get; private set; }

        private Data[] temps;
        private Dictionary<int, GameObject> cachedMonsterIdToGameObject = new Dictionary<int, GameObject>();
        private HashSet<string> monsterPrefabsName = new HashSet<string>();
        private WaveData currentWaveData;
        private int waveIndex;

        private MonsterConfig monsterConfig;
        private LevelData levelData;
        
        public void SetLevel(LevelData levelData, MonsterConfig monsterConfig)
        {
            this.levelData = levelData;
            this.monsterConfig = monsterConfig;
        }

        public async Task Initialize()
        {
            foreach (var waveData in levelData.waves)
            {
                foreach (var spawnData in waveData.spawns)
                {
                    int monsterId = spawnData.monsterId;
                    if (cachedMonsterIdToGameObject.ContainsKey(monsterId))
                        continue;

                    bool found = monsterConfig.TryGetMonsterData(monsterId, out var monsterData);
                    if (!found)
                    {
                        Debug.LogError($"Not found monster data with id '{monsterId}'");

                        return;
                    }

                    GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(monsterData.prefabName);
                    Pool.RegisterPool(go, true);
                    ;

                    monsterPrefabsName.Add(monsterData.prefabName);
                    cachedMonsterIdToGameObject.Add(monsterId, go);
                }
            }

            GameObject bgGo = await AssetBundleManager.GetAsset<GameObject>(levelData.backgroundPrefabName);
            Object.Instantiate(bgGo).transform.position = Vector3.zero;

            LoadWaveData(0);
        }

        private bool LoadWaveData(int wave)
        {
            if (wave >= 0 && wave < levelData.waves.Length)
            {
                waveIndex = wave;

                currentWaveData = levelData.waves[waveIndex];

                SpawnData[] spawnsData = currentWaveData.spawns;

                temps = new Data[spawnsData.Length];

                for (int i = 0; i < spawnsData.Length; i++)
                {
                    SpawnData spawnData = spawnsData[i];

                    temps[i] = new Data(spawnData.startTime, spawnData.endTime, spawnData.totalMonster);
                }

                return true;
            }
            else
            {
                IsCompleted = true;

                return false;
            }
        }

        public void Tick(float deltaTime)
        {
            if (IsPaused || IsCompleted) return;

            bool isWaveCompleted = true;

            for (var i = 0; i < currentWaveData.spawns.Length; i++)
            {
                SpawnData spawnData = currentWaveData.spawns[i];

                Data data = temps[i];

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

                LoadWaveData(waveIndex);

                OnWaveSpawnCompleted?.Invoke(wave);
            }
        }

        private async void Spawn(SpawnData data)
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
            
            GameObject instance = Pool.Instantiate(cachedMonsterIdToGameObject[data.monsterId]);
            
            instance.transform.position = position;

            monsterConfig.TryGetMonsterData(data.monsterId, out var monsterData);
           
            await instance.GetComponent<Monster>().Initialize(monsterData);
        }

        public void Dispose()
        {
            foreach (var pair in cachedMonsterIdToGameObject)
            {
                Pool.UnRegisterPool(pair.Value);
            }

            cachedMonsterIdToGameObject = null;
            
            foreach (var name in monsterPrefabsName)
            {
                AssetBundleManager.UnCache(name);
            }

            monsterPrefabsName = null;
        }

        [Serializable]
        private struct Data
        {
            private int total;

            private readonly float startTime;
            private readonly float endTime;

            private int spawnedCount;
            private float elapsedTime;

            public Data(float startTime, float endTime, int total)
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