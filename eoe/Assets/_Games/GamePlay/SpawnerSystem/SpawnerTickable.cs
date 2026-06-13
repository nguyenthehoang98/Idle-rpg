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
        public Transform[] portals;
        public LevelAsset levelAsset;

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

            return false;
        }

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < currentWaveData.SpawnsData.Length; i++)
            {
                SpawnData spawnData = currentWaveData.SpawnsData[i];
                
                Data data = temps[i];

                if (data.ShouldSpawn(deltaTime)) Spawn(spawnData);

                temps[i] = data;
            }
        }

        private async void Spawn(SpawnData data)
        {
            int portalIndex = data.Portals[0];
            if (data.Portals.Length > 1)
            {
                portalIndex = RandomUtils.Range(0, data.Portals.Length - 1);
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

        private struct Data
        {
            private readonly int total;
            private readonly float interval;
            
            private readonly float startTime;
            private readonly float endTime;

            private int count;
            private float elapsedTime;
            private float intervalElapsedTime;

            public Data(float startTime, float endTime, int total)
            {
                this.startTime = startTime;
                this.endTime = endTime;
                this.interval = (endTime - startTime) / total;
                this.total = total;
                intervalElapsedTime = elapsedTime = 0;
                count = 0;
            }

            public bool ShouldSpawn(float deltaTime)
            {
                if (count > total) return false;
                
                elapsedTime += deltaTime;

                if (elapsedTime >= startTime && elapsedTime <= endTime)
                {
                    intervalElapsedTime += deltaTime;

                    if (intervalElapsedTime >= interval)
                    {
                        intervalElapsedTime = 0;
                        count++;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}