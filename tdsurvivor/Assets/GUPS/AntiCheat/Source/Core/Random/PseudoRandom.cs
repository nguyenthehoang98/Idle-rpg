// System
using System;

namespace GUPS.AntiCheat.Core.Random
{
    /// <summary>
    /// <see cref="IRandomProvider"/> backed by <see cref="System.Random"/>; very fast but predictable to a determined attacker.
    /// </summary>
    public class PseudoRandom : IRandomProvider
    {
        private static readonly System.Random generator = new System.Random();

        /// <inheritdoc cref="IRandomProvider.RandomInt32()"/>
        public int RandomInt32()
        {
            return generator.Next(Int32.MinValue, Int32.MaxValue);
        }

        /// <inheritdoc cref="IRandomProvider.RandomInt32(Int32, Int32)"/>
        public int RandomInt32(Int32 _Min, Int32 _Max)
        {
            return generator.Next(_Min, _Max);
        }
    }
}
