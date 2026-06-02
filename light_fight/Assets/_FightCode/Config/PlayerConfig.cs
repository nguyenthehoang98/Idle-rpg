using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;
using UnityEngine.Serialization;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/PlayerConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/PlayerConfig.asset")]
    public class PlayerConfig : BaseConfig
    {
        [SerializeField] private List<PlayerData> infos = new List<PlayerData>();
    
        private Dictionary<int, PlayerData> cachePlayerData;
    
        public override void OnMapValue()
        {
            cachePlayerData = new Dictionary<int, PlayerData>();

            foreach (var m in infos)
            {
                cachePlayerData.Add(m.Level, m);
            }
        }
        
        public bool TryGetPlayerByLevel(int level, out PlayerData playerData)
        {
            return cachePlayerData.TryGetValue(level, out playerData);
        }
    }

    [Serializable]
    public struct PlayerData
    {
        public int Level;
        public int Exp;
    }
}
