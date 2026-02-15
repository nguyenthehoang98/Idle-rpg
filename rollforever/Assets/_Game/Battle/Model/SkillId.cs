using System;

namespace _Game.Battle
{
    /// <summary>
    /// SkillId format:
    /// id: axxyy
    ///    + a: monster => 1, player => 2
    ///    + xx: 01->99 => rarity/gear
    ///    + yy: 01->99 => skin
    /// </summary>
    [Serializable]
    public struct SkillId : IEquatable<SkillId>
    {
        public int id;
        public int level;
        
        public SkillId(int id, int level)
        {
            this.id = id;
            this.level = level;
        }

        public bool Equals(SkillId other)
        {
            return id == other.id && level == other.level;
        }

        public override bool Equals(object obj)
        {
            return obj is MonsterId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (id * 397) ^ level;
            }
        }

        public static bool operator ==(SkillId left, SkillId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SkillId left, SkillId right)
        {
            return !left.Equals(right);
        }
    }
}