using System;
using System.Collections.Generic;
using _KITSystem.Config;
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
            TextAsset asset =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BattleSource/Configs/MonsterConfig.json");

            MonsterConfig monsterConfig = JsonUtility.FromJson<MonsterConfig>(asset.text);

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
        [SerializeField] private int li;

        public int levelId
        {
            get => li;
            set => li = value;
        }

        [SerializeField] private string ln;

        public string levelName
        {
            get => ln;
            set => ln = value;
        }

        [SerializeField] private string bn;

        public string backgroundName
        {
            get => bn;
            set => bn = value;
        }

        [SerializeField] private WaveData[] ws;

        public WaveData[] waves
        {
            get => ws;
            set => ws = value;
        }

        [JsonProperty] public string[] wavesId { get; set; }
    }

    [Serializable]
    public struct WaveData
    {
        [SerializeField] private string wi;

        public string waveId
        {
            get => wi;
            set => wi = value;
        }

        [SerializeField] private SpawnData[] ss;

        public SpawnData[] spawns
        {
            get => ss;
            set => ss = value;
        }

        [JsonProperty] public string[] groupsId { get; set; }
        
    }

    [Serializable]
    public struct SpawnData
    {
        [SerializeField] private string gi;

        public string groupId
        {
            get => gi;
            set => gi = value;
        }

        [SerializeField] private int mi;

        public int monsterId
        {
            get => mi;
            set => mi = value;
        }

        [SerializeField] private int tt;

        public int total
        {
            get => tt;
            set => tt = value;
        }

        [SerializeField] private float as2;

        public float attackScale
        {
            get => as2;
            set => as2 = value;
        }

        [SerializeField] private float hs2;

        public float healthScale
        {
            get => hs2;
            set => hs2 = value;
        }

        [SerializeField] private float es2;

        public float expScale
        {
            get => es2;
            set => es2 = value;
        }

        [SerializeField] private float r;

        public float radius
        {
            get => r;
            set => r = value;
        }

        [SerializeField] private float st2;

        public float startTime
        {
            get => st2;
            set => st2 = value;
        }

        [SerializeField] private float et2;

        public float endTime
        {
            get => et2;
            set => et2 = value;
        }

        [SerializeField] private int[] ps;

        public int[] portals
        {
            get => ps;
            set => ps = value;
        }
    }
}