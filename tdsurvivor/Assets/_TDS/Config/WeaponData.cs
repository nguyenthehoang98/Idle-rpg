using System;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable]
    public struct WeaponData
    {
        public int weaponId;
        public string name;
        public int skillId;
        public string prefabName;
        public string projectileName;
        public string iconName;
        public string explosionName;
        public string attackAudioName;
        public float attackVolume;
        public int attack;
        public float cooldown;
        public float attackSpeed;
        public float criticalChance;
        public float criticalDamage;
    }
}