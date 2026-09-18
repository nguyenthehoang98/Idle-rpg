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
        public float lifesteal;
        public float skillCooldown;
        public float expMultiplier;
        // Stack đầy thì tự cast skill. <= 0 dùng default của circuit.
        public int stackThreshold;
        // Thời gian power riêng của hero. <= 0 dùng default của circuit.
        public float powerDuration;
        
        public int attackId;
        public int skillId;
    }
}