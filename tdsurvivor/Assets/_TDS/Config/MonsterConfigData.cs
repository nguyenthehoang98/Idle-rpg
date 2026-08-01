using System;

namespace _TDS.Config
{
    [Serializable]
    public struct MonsterConfigData
    {
        public int id;
        public string asset;
        public string name;
        public int health;
        public int attack;
        public int exp;
        public float moveSpeed;
        public float stopDistance;
    }
}