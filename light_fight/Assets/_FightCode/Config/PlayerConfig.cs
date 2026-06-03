using System;
using System.Collections.Generic;
using _KITSystem.Data;
using UnityEngine;

namespace _FightCode.Config
{
    [Serializable]
    public class PlayerConfig : IGameConfig
    {
        [SerializeField] private List<PlayerData> Overview = new List<PlayerData>();
        [SerializeField] private List<PlayerExpData> EXP_1 = new List<PlayerExpData>();

        public void OnMappingValue()
        {
        }

        public void OnPostImported()
        {
        }
    }

    [Serializable]
    public struct PlayerData
    {
        public int ID;
        public string Name;
        public float ExpScale;
        public int BaseHealth;
        public int BaseAttack;
    }

    [Serializable]
    public struct PlayerExpData
    {
        public int Level;
        public int Exp;
    }
}