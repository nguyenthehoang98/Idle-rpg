// System
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GUPS.AntiCheat.Core.Hash
{
    /// <summary>
    /// Static helpers for resolving hash algorithms, computing hashes, comparing them in constant time, and converting between bytes and hex strings.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// Returns the canonical name (for example <c>"SHA-256"</c>) of the supplied hash algorithm.
        /// </summary>
        /// <param name="_HashAlgorithm">The hash algorithm enumeration value.</param>
        /// <returns>The canonical algorithm name, or <c>null</c> for <see cref="EHashAlgorithm.NONE"/>.</returns>
        public static String GetName(EHashAlgorithm _HashAlgorithm)
        {
            switch (_HashAlgorithm)
            {
                case EHashAlgorithm.MD5:
                    return "MD5";
                case EHashAlgorithm.SHA1:
                    return "SHA-1";
                case EHashAlgorithm.SHA256:
                    return "SHA-256";
                case EHashAlgorithm.SHA384:
                    return "SHA-384";
                case EHashAlgorithm.SHA512:
                    return "SHA-512";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a new <see cref="HashAlgorithm"/> instance for the supplied algorithm.
        /// </summary>
        /// <param name="_HashAlgorithm">The hash algorithm enumeration value.</param>
        /// <returns>A new disposable <see cref="HashAlgorithm"/>, or <c>null</c> for <see cref="EHashAlgorithm.NONE"/>.</returns>
        public static HashAlgorithm GetHashAlgorithm(EHashAlgorithm _HashAlgorithm)
        {
            switch (_HashAlgorithm)
            {
                case EHashAlgorithm.MD5:
                    return MD5.Create();
                case EHashAlgorithm.SHA1:
                    return SHA1.Create();
                case EHashAlgorithm.SHA256:
                    return SHA256.Create();
                case EHashAlgorithm.SHA384:
                    return SHA384.Create();
                case EHashAlgorithm.SHA512:
                    return SHA512.Create();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a new <see cref="HashAlgorithm"/> instance for the supplied algorithm name (for example <c>"SHA-256"</c>).
        /// </summary>
        /// <param name="_HashAlgorithm">The algorithm name accepted by <see cref="HashAlgorithm.Create(string)"/>.</param>
        /// <returns>A new disposable <see cref="HashAlgorithm"/>, or <c>null</c> if the name is not recognized.</returns>
        public static HashAlgorithm GetHashAlgorithm(String _HashAlgorithm)
        {
            return HashAlgorithm.Create(_HashAlgorithm);
        }

        /// <summary>
        /// Computes the hash of <paramref name="_Buffer"/> with the specified algorithm.
        /// </summary>
        /// <param name="_HashAlgorithm">The hash algorithm to use.</param>
        /// <param name="_Buffer">The input bytes to hash.</param>
        /// <returns>The computed hash as a byte array.</returns>
        public static byte[] ComputeHash(EHashAlgorithm _HashAlgorithm, byte[] _Buffer)
        {
            using (HashAlgorithm var_HashAlgorithm = GetHashAlgorithm(_HashAlgorithm))
            {
                return var_HashAlgorithm.ComputeHash(_Buffer);
            }
        }

        /// <summary>
        /// Computes the hash of <paramref name="_Buffer"/> with the algorithm identified by name.
        /// </summary>
        /// <param name="_HashAlgorithm">The algorithm name accepted by <see cref="HashAlgorithm.Create(string)"/>.</param>
        /// <param name="_Buffer">The input bytes to hash.</param>
        /// <returns>The computed hash as a byte array.</returns>
        public static byte[] ComputeHash(String _HashAlgorithm, byte[] _Buffer)
        {
            using (HashAlgorithm var_HashAlgorithm = GetHashAlgorithm(_HashAlgorithm))
            {
                return var_HashAlgorithm.ComputeHash(_Buffer);
            }
        }

        /// <summary>
        /// Computes the hash of <paramref name="_Stream"/> with the specified algorithm.
        /// </summary>
        /// <param name="_HashAlgorithm">The hash algorithm to use.</param>
        /// <param name="_Stream">The input stream to read and hash.</param>
        /// <returns>The computed hash as a byte array.</returns>
        public static byte[] ComputeHash(EHashAlgorithm _HashAlgorithm, Stream _Stream)
        {
            using (HashAlgorithm var_HashAlgorithm = GetHashAlgorithm(_HashAlgorithm))
            {
                return var_HashAlgorithm.ComputeHash(_Stream);
            }
        }

        /// <summary>
        /// Computes the hash of <paramref name="_Stream"/> with the algorithm identified by name.
        /// </summary>
        /// <param name="_HashAlgorithm">The algorithm name accepted by <see cref="HashAlgorithm.Create(string)"/>.</param>
        /// <param name="_Stream">The input stream to read and hash.</param>
        /// <returns>The computed hash as a byte array.</returns>
        public static byte[] ComputeHash(String _HashAlgorithm, Stream _Stream)
        {
            using (HashAlgorithm var_HashAlgorithm = GetHashAlgorithm(_HashAlgorithm))
            {
                return var_HashAlgorithm.ComputeHash(_Stream);
            }
        }

        /// <summary>
        /// Compares two hashes for equality in constant time to mitigate timing attacks.
        /// </summary>
        /// <param name="_Hash1">The first hash to compare.</param>
        /// <param name="__Hash2">The second hash to compare.</param>
        /// <returns><c>true</c> if both hashes are non-null, equal in length, and byte-equal; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Intended for cryptographic hash comparison; not a general-purpose byte array comparator.
        /// </remarks>
        public static bool CompareHashes(byte[] _Hash1, byte[] __Hash2)
        {
            if (_Hash1 == null || __Hash2 == null)
                return false;

            if (_Hash1.Length != __Hash2.Length)
                return false;

            // Time-constant XOR-accumulating comparison.
            int var_Result = 0;
            for (int i = 0; i < _Hash1.Length; i++)
            {
                var_Result |= _Hash1[i] ^ __Hash2[i];
            }

            return var_Result == 0;
        }

        /// <summary>
        /// Converts a byte array to its hexadecimal string representation.
        /// </summary>
        /// <param name="_Buffer">The bytes to encode.</param>
        /// <param name="_UpperCase"><c>true</c> for uppercase hex digits, <c>false</c> for lowercase.</param>
        /// <param name="_Separator"><c>true</c> to insert <c>":"</c> between byte pairs.</param>
        /// <returns>The hexadecimal encoding of <paramref name="_Buffer"/>.</returns>
        public static string ToHex(byte[] _Buffer, bool _UpperCase, bool _Separator)
        {
            StringBuilder var_StringBuilder = new StringBuilder(_Buffer.Length * 2);

            for (int i = 0; i < _Buffer.Length; i++)
            {
                var_StringBuilder.Append(_Buffer[i].ToString(_UpperCase ? "X2" : "x2"));
            }

            String var_Result = var_StringBuilder.ToString();

            if (_Separator)
            {
                var_Result = String.Join(":", Regex.Matches(var_Result, ".{2}").Cast<Match>());
            }

            return var_Result;
        }

        /// <summary>
        /// Decodes a hexadecimal string into a byte array.
        /// </summary>
        /// <param name="_Hex">The hexadecimal string to decode.</param>
        /// <param name="_Separator"><c>true</c> if <paramref name="_Hex"/> contains <c>":"</c> separators that should be stripped.</param>
        /// <returns>The decoded bytes.</returns>
        /// <exception cref="ArgumentException">Thrown when the cleaned string has an odd length.</exception>
        public static byte[] FromHex(string _Hex, bool _Separator)
        {
            if (_Separator)
            {
                _Hex = _Hex.Replace(":", string.Empty);
            }

            if (_Hex.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hexadecimal string length.");
            }

            byte[] var_Buffer = new byte[_Hex.Length / 2];

            for (int i = 0; i < var_Buffer.Length; i++)
            {
                var_Buffer[i] = Convert.ToByte(_Hex.Substring(i * 2, 2), 16);
            }

            return var_Buffer;
        }
    }
}
