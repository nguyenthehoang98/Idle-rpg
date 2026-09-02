using System;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct MonsterConfigData
    {
        public int id;
        public string prefabName;

        // Combat
        public int health;
        public int attack;
        public int exp;

        // Movement
        public float moveSpeed;
        public float stopDistance;

        // Presentation
        public string deathAudioClipName;
        public string deathVfxName;

        // Gameplay
        public float damageCooldown;
        public float attackRange;
        public float knockbackResistance;

        // Reward
        public int gold;
    }
}