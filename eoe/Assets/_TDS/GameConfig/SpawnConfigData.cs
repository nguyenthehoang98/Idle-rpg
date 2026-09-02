using System;
using UnityEngine;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct SpawnConfigData
    {
        [NonSerialized] public int[] definitionId;
        public SpawnDefinition definition;
        public int monsterId;
        public int total;
        [NonSerialized] public float[] scales;
        public SpawnScaleDefinition scale;
        public float spawnRadius;
        public float[] spawnsTime;
        public int[] portals;

        public void Parse()
        {
            if (definitionId.Length == 2)
            {
                definition = new SpawnDefinition
                {
                    level = definitionId[0],
                    wave = definitionId[1]
                };
            }
            if (scales.Length == 5)
            {
                scale = new SpawnScaleDefinition
                {
                    attackScale = scales[0],
                    defenseScale = scales[1],
                    healthScale = scales[2],
                    expScale = scales[3],
                    sizeScale = scales[4]
                };
            }
            if (spawnsTime.Length != 2)
                Debug.LogError($"Spawns time must be 2 or more '{JsonUtility.ToJson(definitionId)}'");
            if (portals.Length == 0)
                Debug.LogError($"No portal defined '{JsonUtility.ToJson(definitionId)}'");
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
        public float attackScale;
        public float defenseScale;
        public float healthScale;
        public float expScale;
        public float sizeScale;
    }
}