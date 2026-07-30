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
    /// Drop-in replacement for <see cref="System.Boolean"/> that obfuscates the value in memory and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedBool : IProtected, IDisposable, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Initialization flag for the struct (structs have no default ctor).
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Backing field for <see cref="HasIntegrity"/>.
        /// </summary>
        private bool hasIntegrity;

        /// <summary>
        /// Gets a value indicating whether the protected value still has integrity (i.e. the honeypot has not been tampered with).
        /// </summary>
        public bool HasIntegrity { get => hasIntegrity || !isInitialized; private set => hasIntegrity = value; }

        /// <summary>
        /// The XOR-obfuscated true value, stored as a byte.
        /// </summary>
        private byte obfuscatedValue;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private Int32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private bool fakeValue;

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
        /// Initializes a new protected boolean with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedBool(bool _Value = false)
        {
            this.isInitialized = true;
            this.obfuscatedValue = 0;
            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);
            this.fakeValue = _Value;
            this.hasIntegrity = true;

            this.Obfuscate(_Value);
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public bool Value
        {
            get 
            {
                if(!this.isInitialized)
                {
                    return false;
                }

                if(!this.CheckIntegrity())
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
        /// Stores the given value in XOR-obfuscated form and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(bool _Value)
        {
            byte var_BoolAsByte = (_Value) ? (byte)1 : (byte)0;
            this.obfuscatedValue = (byte)(var_BoolAsByte ^ this.secret);

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private bool UnObfuscate()
        {
            byte var_BoolAsByte = (byte)(this.obfuscatedValue ^ this.secret);
            return (var_BoolAsByte == 1);
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            bool var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            bool var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the obfuscated value and the secret.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedValue = 0;
            this.secret = 0;
        }

        /// <summary>
        /// Returns the hash code of the unobfuscated value.
        /// </summary>
        /// <returns>Hash code of the unobfuscated value.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of the unobfuscated value.
        /// </summary>
        /// <returns>The string representation of the unobfuscated value.</returns>
        public override string ToString()
        {
            return this.Value.ToString();
        }

        #region Serialization

        /// <summary>
        /// Writes the obfuscated value and secret to the given out parameters for player-prefs storage.
        /// </summary>
        /// <param name="_ObfuscatedValue">Receives the obfuscated value.</param>
        /// <param name="_Secret">Receives the secret key.</param>
        internal void Serialize(out byte _ObfuscatedValue, out int _Secret)
        {
            _ObfuscatedValue = this.obfuscatedValue;
            _Secret = this.secret;
        }

        /// <summary>
        /// Restores the obfuscated value and secret from the given parameters.
        /// </summary>
        /// <param name="_ObfuscatedValue">Previously stored obfuscated value.</param>
        /// <param name="_Secret">Previously stored secret key.</param>
        internal void Deserialize(byte _ObfuscatedValue, int _Secret)
        {
            this.obfuscatedValue = _ObfuscatedValue;
            this.secret = _Secret;
            this.fakeValue = this.UnObfuscate();
        }

        #endregion

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a bool in a protected bool.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected bool holding the given value.</returns>
        public static implicit operator ProtectedBool(bool _Value)
        {
            return new ProtectedBool(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected bool to its bool value.
        /// </summary>
        /// <param name="_Value">Protected bool to unwrap.</param>
        /// <returns>The unobfuscated bool value.</returns>
        public static implicit operator bool(ProtectedBool _Value)
        {
            return _Value.Value;
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected booleans represent the same value.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedBool v1, ProtectedBool v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected booleans represent different values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedBool v1, ProtectedBool v2)
        {
            return v1.Value != v2.Value;
        }

        /// <summary>
        /// Returns true when this protected boolean equals the given object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ProtectedBool)
            {
                return this.Value == ((ProtectedBool)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        #endregion
    }
}