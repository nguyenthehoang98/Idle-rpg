using System;
using System.Collections.Generic;
using ExcelExtension;
using UnityEngine;

namespace _FightCode.Config
{
    [ExcelAsset(
        ExcelPath = "Assets/Excels/ResourcesConfig.xlsx",
        ConfigPath = "Assets/_FightSource/Configs/ResourcesConfig.asset")]
    [CreateAssetMenu]
    public class ResourcesConfig : BaseConfig
    {
        [SerializeField] private List<ResourcesData> Overview = new List<ResourcesData>();
    
        private Dictionary<int, ResourcesData> byId;
    
        public override void OnMapValue()
        {
            byId = new Dictionary<int, ResourcesData>();

            foreach (var data in Overview)
            {
                byId.Add(data.ID, data);
            }
        }
        
#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
            }
        }
#endif
    }

    [Serializable]
    public struct ResourcesData : IKitData
    {
        public int ID;
        public string Name;
        
        public void OnImported()
        {
            
        }
    }
}