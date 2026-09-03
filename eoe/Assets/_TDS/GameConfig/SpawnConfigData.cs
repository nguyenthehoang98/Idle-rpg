using System;
using _GameToolkit.Share;
using Newtonsoft.Json;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct SpawnConfigData
    {
        [JsonIgnore] public int[] definitionValues;
        public SpawnDefinition definition;
        public int monsterId;
        public int total;
        [JsonIgnore] private float[] scaleValues;
        public SpawnScaleDefinition scale;
        public float spawnRadius;
        [JsonProperty, SerializeField] private float[] spawnsTime;
        [JsonProperty, SerializeField] private int[] portals;

        [JsonIgnore]
        public int RandomPortal
        {
            get
            {
                if (portals.Length > 1)
                {
                    int index = RandomUtils.Range(0, portals.Length);
                    return portals[index];
                }

                return 0;
            }
        }

        [JsonIgnore]
        public float SpawnStartTime
        {
            get
            {
                if (spawnsTime.Length == 2) return spawnsTime[0];
                return 0;
            }
        }

        [JsonIgnore]
        public float SpawnEndTime
        {
            get
            {
                if (spawnsTime.Length == 2) return spawnsTime[1];
                return float.MaxValue;
            }
        }

        public void Parse()
        {
            if (definitionValues.Length == 2)
            {
                definition = new SpawnDefinition
                {
                    level = definitionValues[0],
                    wave = definitionValues[1]
                };
            }

            if (scaleValues.Length == 4)
            {
                scale = new SpawnScaleDefinition
                {
                    attackMultiplier = scaleValues[0],
                    healthMultiplier = scaleValues[1],
                    expMultiplier = scaleValues[2],
                    sizeMultiplier = scaleValues[3]
                };
            }

            if (spawnsTime.Length != 2)
                Debug.LogError($"Spawns time must be 2 or more '{JsonUtility.ToJson(definitionValues)}'");
            if (portals.Length == 0)
                Debug.LogError($"No portal defined '{JsonUtility.ToJson(definitionValues)}'");
        }
    }

    [Serializable]
    public struct SpawnDefinition
    {
        public int level;
        public int wave;
    }

    [Serializable]
    public struct SpawnScaleDefinition
    {
        public float attackMultiplier;
        public float healthMultiplier;
        public float expMultiplier;
        public float sizeMultiplier;
    }
}