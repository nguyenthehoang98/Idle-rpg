using System;
using System.Collections.Generic;
using _KITSystem.Data;
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
            for (var i = 0; i < Spawn_1.Count; i++)
            {
                var data = Spawn_1[i];
                data.OnImported();
                Spawn_1[i] = data;
            }
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
            }
        }
    }

    [Serializable]
    public struct LevelData
    {
        public int ID; // Id của level
        public int WaveID;
        public int SpawnGroupID;
        [SerializeField, HideInInspector] private string EquipmentsPool;
        [SerializeField, HideInInspector] private string SkillBuffsPool;
        public bool BossWave;
        public int[] EquipmentsID;
        public int[] SkillBuffsID;

        public void OnImported()
        {
            if(!string.IsNullOrEmpty(EquipmentsPool))
            {
                string[] split = EquipmentsPool.Trim('[', ']').Split(',');
                EquipmentsID = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    if (int.TryParse(split[i], out int v))
                    {
                        EquipmentsID[i] = v;
                    }
                    else Debug.LogError("EquipmentsPool is invalid " + ID);
                }                
            }
            
            if(!string.IsNullOrEmpty(SkillBuffsPool))
            {
                string[] split = SkillBuffsPool.Trim('[', ']').Split(',');
                SkillBuffsID = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    if (int.TryParse(split[i], out int v))
                    {
                        SkillBuffsID[i] = v;
                    }
                    else Debug.LogError("SkillBuffsPool is invalid " + ID);
                }                
            }
        }
    }
    
    [Serializable]
    public struct SpawnData
    {
        public int SpawnGroupID;
        public int Power;
        public int MonsterID;
        public int MonsterLevel;
        [SerializeField, HideInInspector] private string SpawnTimes; // [start->end time]
        public float AttackScale;
        public float HealthScale;
        [SerializeField, HideInInspector] private string PortalsID;
        public string Distribute;
        
        /// <summary>
        /// Sẽ có 2 option:
        /// - 1 gia trị => spawn tất cả tại 1 thời điểm
        /// - 2 giá trị => spawn random trong khoảng thời gian đó
        /// </summary>
        public float[] TriggerSpawnTimes;
        public int[] SpawnPortalsID;

        public void OnImported()
        {
            if(!string.IsNullOrEmpty(SpawnTimes))
            {
                string[] split = SpawnTimes.Trim('[', ']').Split(',');
                if (split.Length == 2 || split.Length == 1)
                {
                    TriggerSpawnTimes = new float[split.Length];
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (int.TryParse(split[i], out int v)) TriggerSpawnTimes[i] = v;
                        else Debug.LogError("SpawnTimes is invalid " + SpawnGroupID);
                    }
                }
                else Debug.LogError("SpawnTimes is invalid " + SpawnGroupID);
            }
            else Debug.LogError("SpawnTimes is invalid " + SpawnGroupID);
            
            if(!string.IsNullOrEmpty(PortalsID))
            {
                string[] split = PortalsID.Trim('[', ']').Split(',');
                SpawnPortalsID = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    if (int.TryParse(split[i], out int v)) SpawnPortalsID[i] = v;
                    else Debug.LogError("PortalsID is invalid " + SpawnGroupID);
                }
            }
            else Debug.LogError("PortalsID is invalid " + SpawnGroupID);
        }
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