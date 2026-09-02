using System;

namespace _TDS.GameConfig
{
    [Serializable]
    public struct MonsterConfigData
    {
        public int id;
        public string prefabName;
        public int health;
        public int attack;
        public int exp;
        public float moveSpeed;
        public float stopDistance;
        public string deathAudioClipName;
        public string deathVfxName;
    }
}