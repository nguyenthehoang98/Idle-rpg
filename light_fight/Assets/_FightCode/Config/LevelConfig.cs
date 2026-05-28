    using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/LevelConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/LevelConfig.asset")]
    public class LevelConfig : BaseConfig
    {
        [SerializeField] private List<LevelBatch> spawns = new List<LevelBatch>();

        private Dictionary<int, IReadOnlyDictionary<WaveIdData, LevelBatch>> cacheSpawns;
        
        public override void OnMapValue()
        {
            Dictionary<int, Dictionary<WaveIdData, LevelBatch>> temp = new Dictionary<int, Dictionary<WaveIdData, LevelBatch>>();
            foreach (var data in spawns)
            {
                if (temp.TryGetValue(data.LevelId, out var dictionary))
                {
                    dictionary.Add(new WaveIdData(data.WaveId, data.BatchId), data);
                }
                else
                {
                    dictionary = new Dictionary<WaveIdData, LevelBatch>();
                    dictionary.Add(new WaveIdData(data.WaveId, data.BatchId), data);
                    temp.Add(data.LevelId, dictionary);
                }
            }

            cacheSpawns = new Dictionary<int, IReadOnlyDictionary<WaveIdData, LevelBatch>>();
            foreach (var pair in temp)
            {
                cacheSpawns.Add(pair.Key, pair.Value);
            }
        }

#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                spawns[i].Validate();
            }
        }
#endif

        public bool FindSpawn(int levelID, out IReadOnlyDictionary<WaveIdData, LevelBatch> dictionary)
        {
            return cacheSpawns.TryGetValue(levelID, out dictionary);
        }
    }

    [Serializable]
    public class WaveIdData
    {
        public readonly int WaveId;
        public readonly int BatchId;

        public WaveIdData(int waveId, int batchId)
        {
            WaveId = waveId;
            BatchId = batchId;
        }
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