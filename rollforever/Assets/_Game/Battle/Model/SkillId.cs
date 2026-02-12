using System;

namespace _Game.Battle
{
    public readonly struct SkillId : IEquatable<SkillId>
    {
        public readonly int id;
        public readonly int level;

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
            return HashCode.Combine(id, level);
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