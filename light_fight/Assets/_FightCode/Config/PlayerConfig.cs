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
        [SerializeField] private List<PlayerData> Overview = new List<PlayerData>();
        [SerializeField] private List<PlayerExpData> EXP_1 = new List<PlayerExpData>();
    
        public override void OnMapValue()
        {
            
        }
        
#if UNITY_EDITOR
        public override void OnPostImported()
        {
            for (var i = 0; i < EXP_1.Count; i++)
            {
                var data = EXP_1[i];
                data.OnImported();
                EXP_1[i] = data;
            }
            
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
    public struct PlayerData : IKitData
    {
        public int ID;
        public string Name;
        public float ExpScale;
        public int BaseHealth;
        public int BaseAttack;
        
        public void OnImported()
        {
            
        }
    }

    [Serializable]
    public struct PlayerExpData: IKitData
    {
        public int Level;
        public int Exp;

        public void OnImported()
        {

        }
    }
}
