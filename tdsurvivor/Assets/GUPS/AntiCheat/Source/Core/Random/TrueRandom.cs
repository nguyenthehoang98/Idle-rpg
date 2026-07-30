// System
using System;

namespace GUPS.AntiCheat.Core.Random
{
    /// <summary>
    /// <see cref="IRandomProvider"/> backed by a cryptographic RNG; slower than <see cref="PseudoRandom"/> but unpredictable.
    /// </summary>
    /// <remarks>
    /// Out-of-range samples are clamped to <c>[_Min, _Max - 1]</c>, which biases the distribution at the bounds.
    /// </remarks>
    public class TrueRandom : IRandomProvider
    {
        private static readonly System.Security.Cryptography.RandomNumberGenerator generator = new System.Security.Cryptography.RNGCryptoServiceProvider();

        /// <inheritdoc cref="IRandomProvider.RandomInt32()"/>
        public int RandomInt32()
        {
            return this.RandomInt32(Int32.MinValue, Int32.MaxValue);
        }

        /// <inheritdoc cref="IRandomProvider.RandomInt32(Int32, Int32)"/>
        public int RandomInt32(Int32 _Min, Int32 _Max)
        {
            byte[] var_Bytes = new byte[sizeof(Int32)];

            generator.GetBytes(var_Bytes);

            Int32 var_Value = BitConverter.ToInt32(var_Bytes, 0);

            // Clamp into [_Min, _Max - 1] - biases the distribution at the bounds.
            if(var_Value < _Min)
            {
                var_Value = _Min;
            }
            else if (var_Value > _Max - 1)
            {
                var_Value = _Max - 1;
            }

            return var_Value;
        }
    }
}
