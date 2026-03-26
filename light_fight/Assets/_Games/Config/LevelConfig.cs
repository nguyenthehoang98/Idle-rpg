using System;
using System.Collections.Generic;
using _KIT.Config;
using _KIT.Config.ExcelExtension.Runtime;
using UnityEngine;

namespace _Games.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/LevelConfig.xlsx",
        ConfigPath = "Assets/_Sources/Configs/LevelConfig.asset")]
    public class LevelConfig : KitBaseConfig
    {
        [SerializeField] private List<LevelData> datas = new List<LevelData>();
        [SerializeField] private List<LevelBatch> spawns = new List<LevelBatch>();

        private Dictionary<int, LevelData> cacheData;
        private Dictionary<int, IReadOnlyDictionary<Vector2Int, LevelBatch>> cacheSpawns;
        
        public override void OnMapValue()
        {
            cacheData = new Dictionary<int, LevelData>();
            foreach (var data in datas)
            {
                cacheData.Add(data.LevelId, data);
            }
            
            Dictionary<int, Dictionary<Vector2Int, LevelBatch>> temp = new Dictionary<int, Dictionary<Vector2Int, LevelBatch>>();
            foreach (var data in spawns)
            {
                if (temp.TryGetValue(data.LevelId, out var dictionary))
                {
                    dictionary.Add(new Vector2Int(data.WaveId, data.BatchId), data);
                }
                else
                {
                    dictionary = new Dictionary<Vector2Int, LevelBatch>();
                    dictionary.Add(new Vector2Int(data.WaveId, data.BatchId), data);
                    temp.Add(data.LevelId, dictionary);
                }
            }

            cacheSpawns = new Dictionary<int, IReadOnlyDictionary<Vector2Int, LevelBatch>>();
            foreach (var pair in temp)
            {
                cacheSpawns.Add(pair.Key, pair.Value);
            }
        }

        public override void OnPostImported()
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                spawns[i].Validate();
            }
        }

        public bool FindData(int levelID, out LevelData levelData)
        {
            return cacheData.TryGetValue(levelID, out levelData);
        }

        public bool FindSpawn(int levelID, out IReadOnlyDictionary<Vector2Int, LevelBatch> dictionary)
        {
            return cacheSpawns.TryGetValue(levelID, out dictionary);
        }
    }

    [Serializable]
    public class LevelData
    {
        [SerializeField] private int levelId;
        [SerializeField] private string levelDesign;

        public int LevelId => levelId;
        public string LevelDesign => levelDesign;
    }
    
    [Serializable]
    public class LevelBatch
    {
        [SerializeField] private int levelId;
        [SerializeField] private int waveId;
        [SerializeField] private int batchId;
        [SerializeField] private int isBoss;
        [SerializeField] private float delayTime;
        [SerializeField] private float duration;
        [SerializeField] private int power;
        [SerializeField] private string spawnId;
        [SerializeField] private string[] data;
        [SerializeField] private Vector2Int[] weights;

        public int LevelId => levelId;
        public int WaveId => waveId;
        public int BatchId => batchId;
        public int IsBoss => isBoss;
        public float DelayTime => delayTime;
        public float Duration => duration;
        public Vector2Int[] Weights => weights;
        public int Power => power;

        public void Validate()
        {
            weights = new Vector2Int[data.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                var split = data[i].Split('_');
                if (split.Length != 2)
                {
                    Debug.LogError($"Config error. length={split.Length}, format={data[i]}");
                    continue;
                }
                if (!int.TryParse(split[0], out int num1) || !int.TryParse(split[1], out int num2))
                {
                    Debug.LogError("Error parse, format=" + data[i]);
                    continue;
                }
                weights[i] = new Vector2Int(num1, num2);
            }
        }
    }
}