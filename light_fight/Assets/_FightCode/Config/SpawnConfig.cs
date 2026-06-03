using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class SpawnConfig : IGameConfig
    {
        [SerializeField] private List<SpawnData> spawns = new List<SpawnData>();
    
        private Dictionary<int, List<SpawnData>> byId;
    
        public void OnMappingValue()
        {
            byId = new Dictionary<int, List<SpawnData>>();

            foreach (var spawnData in spawns)
            {
                if (byId.TryGetValue(spawnData.SpawnGroupID, out List<SpawnData> list))
                    list.Add(spawnData);
                else byId.Add(spawnData.SpawnGroupID, new List<SpawnData> { spawnData });
            }
        }

        public void OnPostImported()
        {
        }

        public bool TryGetSpawnById(int groupID, out List<SpawnData> list)
        {
            return byId.TryGetValue(groupID, out list);
        }
    }

    // Tính tổng HP mỗi wave. để tính xem có win đc ko
    [Serializable]
    public struct SpawnData
    {
        public int SpawnGroupID;
        public int Power;
        public int MonsterID;
        public int MonsterLevel;
        public float[] SpawnTimes; // [start->end time]
        public float AttackScale;
        public float HealthScale;
        public int[] PortalsID;
        public string Distribute;
    }
}