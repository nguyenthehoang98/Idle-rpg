using System;

namespace _Game.Battle
{
    public readonly struct MonsterId : IEquatable<MonsterId>
    {
        public readonly int id;
        public readonly int level;

        public MonsterId(int id, int level)
        {
            this.id = id;
            this.level = level;
        }

        public bool Equals(MonsterId other)
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

        public static bool operator ==(MonsterId left, MonsterId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MonsterId left, MonsterId right)
        {
            return !left.Equals(right);
        }
    }
}