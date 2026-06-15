using System;

namespace _Games.GamePlay.SpawnerSystem
{
    [Serializable]
    public struct SpawnData
    {
        public string Monster;
        public int Total;
        public float Radius;
        public float SpawnStartTime;
        public float SpawnEndTime;
        public int[] Portals;
    }

    [Serializable]
    public struct WaveData
    {
        public SpawnData[] SpawnsData;
    }
}