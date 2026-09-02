using System;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct HeroConfigData
    {
        public int id;
        public string prefabName;
        
        public int health;
        public int attack;
        public float attackRange;
        public float attackSpeed;
        public float critChance;
        public float critDamage;
        public float skillCooldown;
        public float expMultiplier;
        
        public int attackId;
        public int skillId;
        public int ultimateId;
    }
}