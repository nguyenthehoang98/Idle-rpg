// System
using System;

namespace GUPS.AntiCheat.Core.Random
{
    /// <summary>
    /// Provides random 32-bit integers.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// Returns a random integer in the range <c>[<see cref="Int32.MinValue"/>, <see cref="Int32.MaxValue"/>)</c>.
        /// </summary>
        /// <returns>A random 32-bit signed integer.</returns>
        Int32 RandomInt32();

        /// <summary>
        /// Returns a random integer in the range <c>[<paramref name="_Min"/>, <paramref name="_Max"/>)</c>.
        /// </summary>
        /// <param name="_Min">Inclusive lower bound.</param>
        /// <param name="_Max">Exclusive upper bound.</param>
        /// <returns>A random 32-bit signed integer within the range.</returns>
        Int32 RandomInt32(Int32 _Min, Int32 _Max);
    }

}
