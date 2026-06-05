using System;
using System.Collections.Generic;
using _KITSystem.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class LevelConfig : IGameConfig
    {
        [SerializeField] private List<LevelData> Overview = new List<LevelData>();
        [SerializeField] private List<SpawnData> Spawn_1 = new List<SpawnData>();
        [SerializeField] private List<EnvironmentData> Environment_2 = new List<EnvironmentData>();
        [SerializeField] private List<PortalData> Portal_3 = new List<PortalData>();
    
        public void OnMappingValue()
        {
        }
        
        public void OnPostImported()
        {
        }
    }

    [Serializable]
    public struct LevelData
    {
        public int ID; // Id của level
        public int WaveID;
        public int SpawnGroupID;
        public int[] EquipmentsPool;
        public int[] SkillBuffsPool;
        public bool BossWave;
    }
    
    [Serializable]
    public struct SpawnData
    {
        public int SpawnGroupID;
        public int Power;
        public int MonsterID;
        public int MonsterLevel;
        /// <summary>
        /// Sẽ có 2 option:
        /// - 1 gia trị => spawn tất cả tại 1 thời điểm
        /// - 2 giá trị => spawn random trong khoảng thời gian đó
        /// </summary>
        public float[] SpawnTimes; // [start->end time]
        public float AttackScale;
        public float HealthScale;
        public int[] PortalsID;
        public string Distribute;
    }

    [Serializable]
    public struct EnvironmentData
    {
        public int Level;
        public string Path;
    }

    [Serializable]
    public struct PortalData
    {
        public int ID;
        public string Path;
    }
}