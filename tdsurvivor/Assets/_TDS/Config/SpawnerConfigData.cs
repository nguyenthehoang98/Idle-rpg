using System;

namespace _TDS.Config
{
    [Serializable]
    public struct SpawnerConfigData
    {
        public int level;
        public int wave;
        public int monster;
        public int totalMonster;
        public float healthScale;
        public float attackScale;
        public float expScale;
        public float sizeScale;
        public float spawnStartTime;
        public float spawnEndTime;
        public float spawnAreaRadius;
        public int[] portals;
    }
}