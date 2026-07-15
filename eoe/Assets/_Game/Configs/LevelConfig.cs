using System;
using System.Collections.Generic;
using System.Text;
using _KITSystem.Config;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
                data.spawns = new SpawnData[data.groupsId.Length];

                for (var j = 0; j < data.groupsId.Length; j++)
                {
                    string spawnGroupId = data.groupsId[j];
                    bool found = false;
                    foreach (var spawnData in spawns)
                    {
                        if (string.Equals(spawnData.groupId, spawnGroupId))
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
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BattleSource/Configs/MonsterConfig.json");

            byte[] unpick = LZ4Pickler.Unpickle(asset.bytes); 

            string text = Encoding.UTF8.GetString(unpick);
            
            MonsterConfig monsterConfig = JsonUtility.FromJson<MonsterConfig>(text);

            monsterConfig.OnMappingValue();

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData levelData = levels[i];

                foreach (var waveData in levelData.waves)
                {
                    foreach (var spawnData in waveData.spawns)
                    {
                        if (monsterConfig.TryGetMonsterData(spawnData.monsterId, out MonsterData monsterData)) continue;

                        Debug.LogError($"Not found monster id '{spawnData.monsterId}' at spawn group id '{waveData.groupsId}'");
                    }
                }
            }
#endif
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
        public string backgroundName;
        public WaveData[] waves;
        [JsonProperty, HideInInspector] public string[] wavesId;
    }

    [Serializable]
    public struct WaveData
    {
        public string waveId;
        [JsonProperty, HideInInspector] public string[] groupsId;
        public SpawnData[] spawns;
    }

    [Serializable]
    public struct SpawnData
    {
        public string groupId;
        public int monsterId;
        public int total;
        public float attackScale;
        public float healthScale;
        public float expScale;
        public float radius;
        public float startTime;
        public float endTime;
        public int[] portals;
    }
}