using System;
using System.Collections.Generic;
using _KITSystem.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class PlayerConfig : IGameConfig
    {
        [SerializeField] private List<PlayerData> Overview = new List<PlayerData>();

        public void OnMappingValue()
        {
        }

        public void OnPostImported()
        {
            for (var i = 0; i < Overview.Count; i++)
            {
                var data = Overview[i];
                data.OnImported();
                Overview[i] = data;
            }
        }
    }

    [Serializable]
    public struct PlayerData
    {
        public int ID;
        public string Name;
        
        [JsonProperty] private int Health_A;
        [JsonProperty] private int Health_B;
        [JsonProperty] private int Attack_A;
        [JsonProperty] private int Attack_B;
        [JsonProperty] private int Exp_A;
        [JsonProperty] private int Exp_B;
        
        [JsonIgnore] public LinearFormula Exp;
        [JsonIgnore] public LinearFormula Attack;
        [JsonIgnore] public LinearFormula Health;
        
        public void OnImported()
        {
            Exp = new LinearFormula(1, Exp_A, Exp_B);
            Attack = new LinearFormula(1, Attack_A, Attack_B);
            Health = new LinearFormula(1, Health_A, Health_B);
        }
    }
}