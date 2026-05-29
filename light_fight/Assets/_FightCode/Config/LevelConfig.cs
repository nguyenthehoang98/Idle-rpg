    using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;
using UnityEngine.Serialization;

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
            Dictionary<int, Dictionary<WaveIdData, LevelBatch>> temp =
                new Dictionary<int, Dictionary<WaveIdData, LevelBatch>>();
            foreach (var data in spawns)
            {
                if (temp.TryGetValue(data.levelId, out var dictionary))
                {
                    dictionary.Add(new WaveIdData(data.waveId, data.batchId), data);
                }
                else
                {
                    dictionary = new Dictionary<WaveIdData, LevelBatch>();
                    dictionary.Add(new WaveIdData(data.waveId, data.batchId), data);
                    temp.Add(data.levelId, dictionary);
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
    public struct LevelBatch
    {
        public int levelId;
        public int waveId;
        public int batchId;
        public int isBoss;
        public float delayTime;
        public float duration;
        public int power;
        public Vector2Int[] weights;
        [SerializeField] private string[] data;

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