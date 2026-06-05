using System;
using System.Collections.Generic;
using _KITSystem.Config;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class StatConfig : IGameConfig
    {
        [SerializeField] private List<StatData> Overview = new List<StatData>();
    
        private Dictionary<int, StatData> byId;
    
        public void OnMappingValue()
        {
            byId = new Dictionary<int, StatData>();

            foreach (var statData in Overview)
            {
                byId.Add(statData.ID, statData);
            }
        }
        
        public void OnPostImported()
        {
        }
    }

    [Serializable]
    public struct StatData 
    {
        public int ID;
        public string Name;
        public float Value;
        public float Modifier;
    }
}