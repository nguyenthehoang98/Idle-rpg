using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Games.GamePlay.AnimationSystem;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Games.GamePlay.SpawnerSystem
{
    [Serializable]
    public class SpawnerTickable : ITickable
    {
        public event Action<int> OnSpawnCompleted;

        public Transform[] portals;
        public LevelAsset levelAsset;

        public bool IsPaused { get; set; } = false;
        public bool IsCompleted { get; private set; } = false;

        private Data[] temps;
        private HashSet<GameObject> monsterObjects = new HashSet<GameObject>();
        private HashSet<string> monsterNames = new HashSet<string>();
        private WaveData currentWaveData;
        private int waveIndex = 0;

        public async Task Initialize()
        {
            foreach (var waveData in levelAsset.WavesData)
            {
                foreach (var spawnData in waveData.SpawnsData)
                {
                    if (monsterNames.Add(spawnData.Monster))
                    {
                        GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(spawnData.Monster);
                        Pool.RegisterPool(go, true);
                        monsterObjects.Add(go);
                    }
                }
            }

            Object.Instantiate(levelAsset.backgroundPrefab).transform.position = Vector3.zero;

            LoadWaveData(0);
        }

        private bool LoadWaveData(int wave)
        {
            if (wave >= 0 && wave < levelAsset.WavesData.Length)
            {
                waveIndex = wave;

                currentWaveData = levelAsset.WavesData[waveIndex];

                SpawnData[] spawnsData = currentWaveData.SpawnsData;

                temps = new Data[spawnsData.Length];

                for (int i = 0; i < spawnsData.Length; i++)
                {
                    SpawnData spawnData = spawnsData[i];

                    temps[i] = new Data(spawnData.SpawnStartTime, spawnData.SpawnEndTime, spawnData.Total);
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

            for (var i = 0; i < currentWaveData.SpawnsData.Length; i++)
            {
                SpawnData spawnData = currentWaveData.SpawnsData[i];

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

                OnSpawnCompleted?.Invoke(wave);
            }
        }

        private async void Spawn(SpawnData data)
        {
            int portalIndex = data.Portals[0];
            
            if (data.Portals.Length > 1)
            {
                portalIndex = RandomUtils.Range(0, data.Portals.Length);
            }

            Vector3 position = portals[portalIndex].position + new Vector3(
                RandomUtils.Range(-data.Radius, data.Radius),
                RandomUtils.Range(-data.Radius, data.Radius)
            );


            GameObject go = await AssetBundleManager.GetAssetCached<GameObject>(data.Monster);

            GameObject instance = Pool.Instantiate(go);
            instance.transform.position = position;

            instance.GetComponent<Monster>().Initialize();
        }

        public void Dispose()
        {
            foreach (var go in monsterObjects)
            {
                Pool.UnRegisterPool(go);
            }

            foreach (var name in monsterNames)
            {
                AssetBundleManager.UnCache(name);
            }
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