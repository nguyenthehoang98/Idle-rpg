using System;

namespace _TDS.GameConfig
{
    public enum MonsterRank
    {
        Normal,
        Elite,
        Boss,
    }

    [Serializable]
    public struct MonsterSkillConfigData
    {
        public int skillId;
        public float cooldown;
        public float damageMultiplier;
    }

    [Serializable]
    public struct MonsterConfigData
    {
        public int id;
        public string prefabName;

        // Combat
        public MonsterRank rank;
        public int health;
        public int attack;
        public int exp;
        public MonsterSkillConfigData[] skills;

        // Movement
        public float moveSpeed;
        public float stopDistance;

        // Gameplay
        public float damageCooldown;
        public float attackRange;
        public float knockbackResistance;

        // Reward
        public int gold;
    }
}