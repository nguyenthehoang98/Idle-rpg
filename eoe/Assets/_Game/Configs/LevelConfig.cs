using System;
using System.Collections.Generic;
using _KITSystem.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace _Game.Configs
{
    [Serializable]
    public class LevelConfig : IGameConfig
    {
        [SerializeField, JsonProperty] private List<LevelData> levels = new List<LevelData>();
        [JsonProperty] private List<WaveData> waves = new List<WaveData>();
        [JsonProperty] private List<SpawnData> spawns = new List<SpawnData>();

        private Dictionary<int, LevelData> cached;
        
        public void OnMappingValue()
        {
            cached = new Dictionary<int, LevelData>();

            foreach (var data in levels)
            {  
                if (!cached.TryAdd(data.levelId, data)) Debug.LogError($"Duplicate level '{data.levelId}'");
            }
        }

        public void OnPostImported()
        {
            for (int i = 0; i < waves.Count; i++)
            {
                WaveData data = waves[i];
                data.spawns = new SpawnData[data.spawnGroupId.Length];
                
                for (var j = 0; j < data.spawnGroupId.Length; j++)
                {
                    string spawnGroupId = data.spawnGroupId[j];
                    bool found = false;
                    foreach (var spawnData in spawns)
                    {
                        if (string.Equals(spawnData.spawnGroupId, spawnGroupId))
                        {
                            found = true;
                            data.spawns[j] = spawnData;
                            break;
                        }
                    }

                    if (!found)
                        Debug.LogError($"Not found spawn group at wave '{data.waveId}', group id '{spawnGroupId}'");
                }

                waves[i] = data;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData data = levels[i];
                data.waves = new WaveData[data.wavesId.Length];

                for (var j = 0; j < data.wavesId.Length; j++)
                {
                    var waveId = data.wavesId[j];
                    bool found = false;
                    foreach (var waveData in waves)
                    {
                        if (string.Equals(waveData.waveId, waveId))
                        {
                            data.waves[j] = waveData;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        Debug.LogError($"Not found level '{data.levelId}', wave '{waveId}'");
                }

                levels[i] = data;
            }
        }

        public void OnValidateLinkConfig()
        {
        }

        public bool TryGetLevelData(int levelId, out LevelData levelData)
        {
            return cached.TryGetValue(levelId, out levelData);
        }
    }

    [Serializable]
    public struct LevelData
    {
        public int levelId;
        public string levelName;
        public string backgroundPrefabName;
        public WaveData[] waves;
        [JsonProperty, HideInInspector] public string[] wavesId;
    }

    [Serializable]
    public struct WaveData
    {
        public string waveId;
        [JsonProperty, HideInInspector] public string[] spawnGroupId;
        public SpawnData[] spawns;
    }

    [Serializable]
    public struct SpawnData
    {
        public string spawnGroupId;
        public int monsterId;
        public int totalMonster;
        public float monsterAttackScale;
        public float monsterHealthScale;
        public float monsterExpScale;
        public float spawnRadius;
        public float startTime;
        public float endTime;
        public int[] portals;
    }
}