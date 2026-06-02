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
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
    
        private Dictionary<int, List<LevelData>> byId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, List<LevelData>>();

            foreach (var levelData in levels)
            {
                if (byId.TryGetValue(levelData.ID, out var list))
                    list.Add(levelData);
                else byId.Add(levelData.ID, new List<LevelData> { levelData });
            }
        }

        public bool TryGetLevelsById(int levelId, out List<LevelData> list)
        {
            return byId.TryGetValue(levelId, out list);
        }
    }

    [Serializable]
    public struct LevelData
    {
        public int ID; // Id của level
        public int WaveID;
        public int SpawnGroupID;
        public int[] EquipmentsPool;
        public int[] BuffsPool;
        public int[] CUR;
        public int BossWave;
        public int EnvironmentID;
    }
}