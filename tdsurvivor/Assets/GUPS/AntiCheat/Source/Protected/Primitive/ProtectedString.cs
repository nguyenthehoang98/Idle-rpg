// System
using System;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Protected;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector;
using GUPS.AntiCheat.Settings;

namespace GUPS.AntiCheat.Protected
{
    /// <summary>
    /// Drop-in replacement for <see cref="System.String"/> that stores its value encrypted (UTF-8 + Base64 with a per-instance secret) and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <remarks>
    /// String protection is noticeably more expensive than the numeric primitives; reserve it for actually sensitive values rather than every string in the project.
    /// </remarks>
    /// <example>
    /// Use it as a drop-in replacement for <see cref="System.String"/>:
    /// <code>
    /// using GUPS.AntiCheat.Protected;
    ///
    /// ProtectedString playerName = "Tim";
    /// playerName += " (admin)";                            // operator + works
    /// if (playerName == "Tim (admin)") { /* ... */ }       // equality works
    /// string raw = playerName;                             // implicit conversion to string
    /// </code>
    /// </example>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedString : IProtected, IDisposable, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Initialization flag for the struct (structs have no default ctor).
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Encrypts the given string with the secret and returns a Base64-encoded UTF-8 payload.
        /// </summary>
        /// <param name="_String">String to protect.</param>
        /// <param name="_Secret">Secret key used for encryption.</param>
        /// <returns>The encrypted, Base64-encoded representation, or <c>null</c>/empty if the input was <c>null</c>/empty.</returns>
        private static string EncryptToUTF8(string _String, int _Secret)
        {
            if (_String == null)
            {
                return null;
            }

            if (_String.Length == 0)
            {
                return "";
            }

            uint key1 = 0x45435345 + (uint)_Secret;
            uint key2 = 0x95656543;

            byte[] buff1 = System.Text.UTF8Encoding.UTF8.GetBytes(_String);
            byte[] buff = new byte[buff1.Length + 1];

            buff[0] = (byte)(GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue) % 256);

            byte d = buff[0];

            for (int i = 1; i < buff.Length; i++)
            {
                buff[i] = buff1[i - 1];
                key1 = (key1 * 4343255 + d + 5235457) % 0xFFFFFFFE;
                key2 = (key2 * 5354354 + d + 22646641) % 0xFFFFFFFE;

                d = buff[i];

                buff[i] = (byte)((uint)buff[i] ^ key1);
                buff[i] = (byte)((byte)buff[i] + (byte)key2);
            }

            return System.Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Reverses <see cref="EncryptToUTF8"/> and returns the original string.
        /// </summary>
        /// <param name="_String">Encrypted Base64 payload.</param>
        /// <param name="_Secret">Secret key originally used for encryption.</param>
        /// <returns>The decrypted string, or <c>null</c>/empty if the input was <c>null</c>/empty.</returns>
        private static string DecryptFromUTF8(string _String, int _Secret)
        {
            if (_String == null)
            {
                return null;
            }

            if (_String.Length == 0)
            {
                return "";
            }

            uint key1 = 0x45435345 + (uint)_Secret;
            uint key2 = 0x95656543;

            byte[] buff1 = System.Convert.FromBase64String(_String);
            byte[] buff = new byte[buff1.Length - 1];

            byte d = buff1[0];

            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = buff1[i + 1];
                key1 = (key1 * 4343255 + d + 5235457) % 0xFFFFFFFE;
                key2 = (key2 * 5354354 + d + 22646641) % 0xFFFFFFFE;

                buff[i] = (byte)((byte)buff[i] - (byte)key2);
                buff[i] = (byte)((uint)buff[i] ^ key1);
                d = buff[i];
            }

            return System.Text.UTF8Encoding.UTF8.GetString(buff, 0, buff.Length);
        }

        /// <summary>
        /// Backing field for <see cref="HasIntegrity"/>.
        /// </summary>
        private bool hasIntegrity;

        /// <summary>
        /// Gets a value indicating whether the protected value still has integrity (i.e. the honeypot has not been tampered with).
        /// </summary>
        public bool HasIntegrity { get => hasIntegrity || !isInitialized; private set => hasIntegrity = value; }

        /// <summary>
        /// The encrypted true value.
        /// </summary>
        private string obfuscatedValue;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private Int32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private string fakeValue;

        /// <summary>
        /// Unity serialization hook; writes the true value into the honeypot field.
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.fakeValue = Value;
        }

        /// <summary>
        /// Unity deserialization hook; rebuilds the obfuscation state from the deserialized honeypot value.
        /// </summary>
        public void OnAfterDeserialize()
        {
            this = this.fakeValue;
        }

        /// <summary>
        /// Initializes a new protected string with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedString(string _Value = null)
        {
            this.isInitialized = true;
            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, +5432);
            this.obfuscatedValue = EncryptToUTF8(_Value, this.secret);

            this.hasIntegrity = true;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public string Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return null;
                }

                if (!this.CheckIntegrity())
                {
                    AntiCheatMonitor.Instance.GetDetector<PrimitiveCheatingDetector>()?.OnNext(this);
                }

                return this.UnObfuscate();
            }
            set { this.Obfuscate(value); }
        }

        /// <inheritdoc cref="IProtected.Value"/>
        object IProtected.Value => this.Value;

        /// <summary>
        /// Stores the given value in encrypted form and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(string _Value)
        {
            this.obfuscatedValue = EncryptToUTF8(_Value, this.secret);

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private string UnObfuscate()
        {
            return DecryptFromUTF8(this.obfuscatedValue, this.secret);
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            string var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, +5432);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the encrypted value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            string var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the encrypted value and the secret.
        /// </summary>
        public void Dispose()
        {
            this.secret = 0;
            this.obfuscatedValue = null;
        }

        /// <summary>
        /// Returns the unobfuscated string value.
        /// </summary>
        /// <returns>The unobfuscated string.</returns>
        public override string ToString()
        {
            return this.Value;
        }

        /// <summary>
        /// Returns the hash code of the unobfuscated value.
        /// </summary>
        /// <returns>Hash code of the unobfuscated value, or <c>0</c> if the value is <c>null</c>.</returns>
        public override int GetHashCode()
        {
            if (this.Value == null)
            {
                return 0;
            }

            return this.Value.GetHashCode();
        }

        #region Serialization

        /// <summary>
        /// Writes the obfuscated value and secret to the given out parameters for player-prefs storage.
        /// </summary>
        /// <param name="_ObfuscatedValue">Receives the encrypted value.</param>
        /// <param name="_Secret">Receives the secret key.</param>
        internal void Serialize(out String _ObfuscatedValue, out int _Secret)
        {
            _ObfuscatedValue = this.obfuscatedValue;
            _Secret = this.secret;
        }

        /// <summary>
        /// Restores the obfuscated value and secret from the given parameters.
        /// </summary>
        /// <param name="_ObfuscatedValue">Previously stored encrypted value.</param>
        /// <param name="_Secret">Previously stored secret key.</param>
        internal void Deserialize(String _ObfuscatedValue, int _Secret)
        {
            this.obfuscatedValue = _ObfuscatedValue;
            this.secret = _Secret;
            this.fakeValue = this.UnObfuscate();
        }

        #endregion

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a string in a protected string.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected string holding the given value.</returns>
        public static implicit operator ProtectedString(string _Value)
        {
            return new ProtectedString(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected string to its string value.
        /// </summary>
        /// <param name="_Value">Protected string to unwrap.</param>
        /// <returns>The unobfuscated string value.</returns>
        public static implicit operator string(ProtectedString _Value)
        {
            return _Value.Value;
        }

        #endregion

        #region Calculation operator

        /// <summary>
        /// Concatenates two protected strings.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>A new protected string holding the concatenation of both values.</returns>
        public static ProtectedString operator +(ProtectedString v1, ProtectedString v2)
        {
            return new ProtectedString(v1.Value + v2.Value);
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected strings represent the same text.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedString v1, ProtectedString v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected strings represent different text.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedString v1, ProtectedString v2)
        {
            return v1.Value != v2.Value;
        }

        /// <summary>
        /// Returns true when this protected string equals the given object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ProtectedString)
            {
                return this.Value == ((ProtectedString)obj).Value;
            }

            if (this.Value == null && obj == null)
            {
                return true;
            }

            return this.Value.Equals(obj);
        }

        #endregion
    }
}