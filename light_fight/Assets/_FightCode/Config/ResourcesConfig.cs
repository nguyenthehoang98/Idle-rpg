using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class ResourcesConfig : IGameConfig
    {
        [SerializeField] private List<ResourcesData> Overview = new List<ResourcesData>();

        private Dictionary<int, ResourcesData> byId;

        public void OnMappingValue()
        {
            byId = new Dictionary<int, ResourcesData>();

            foreach (var data in Overview)
            {
                byId.Add(data.ID, data);
            }
        }

        public void OnPostImported()
        {
        }
    }

    [Serializable]
    public struct ResourcesData
    {
        public int ID;
        public string Name;
    }
}