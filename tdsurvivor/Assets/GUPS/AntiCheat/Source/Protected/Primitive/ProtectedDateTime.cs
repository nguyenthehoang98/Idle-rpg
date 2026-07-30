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
    /// Drop-in replacement for <see cref="System.DateTime"/> that protects the underlying <see cref="DateTime.Ticks"/> via a <see cref="ProtectedInt64"/> and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedString"/>
    [Serializable]
    public struct ProtectedDateTime : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The obfuscated <see cref="DateTime.Ticks"/> value, stored as a <see cref="ProtectedInt64"/>.
        /// </summary>
        private ProtectedInt64 obfuscatedInt64;

        /// <summary>
        /// Honeypot value (ticks) serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private Int64 fakeValue;

        /// <summary>
        /// Unity serialization hook; writes the true value into the honeypot field.
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.fakeValue = Value.Ticks;
        }

        /// <summary>
        /// Unity deserialization hook; rebuilds the obfuscation state from the deserialized honeypot value.
        /// </summary>
        public void OnAfterDeserialize()
        {
            this = new DateTime(this.fakeValue);
        }

        /// <summary>
        /// Initializes a new protected DateTime with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedDateTime(DateTime _Value)
        {
            this.isInitialized = true;
            this.obfuscatedInt64 = 0;
            this.fakeValue = 0;
            this.hasIntegrity = true;

            this.Obfuscate(_Value);
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public DateTime Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return new DateTime();
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
        /// Stores the given value's ticks in the inner <see cref="ProtectedInt64"/> and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(DateTime _Value)
        {
            this.obfuscatedInt64.Value = _Value.Ticks;

            this.fakeValue = _Value.Ticks;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private DateTime UnObfuscate()
        {
            return new DateTime(this.obfuscatedInt64.Value);
        }

        /// <summary>
        /// Rerolls the inner <see cref="ProtectedInt64"/>'s secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            this.obfuscatedInt64.Obfuscate();
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            DateTime var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue.Ticks)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the inner <see cref="ProtectedInt64"/>.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedInt64.Dispose();
        }

        /// <summary>
        /// Returns the string representation of the unobfuscated value.
        /// </summary>
        /// <returns>The string representation of the unobfuscated value.</returns>
        public override string ToString()
        {
            return this.Value.ToString();
        }

        /// <summary>
        /// Returns the hash code of the unobfuscated value.
        /// </summary>
        /// <returns>Hash code of the unobfuscated value.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a <see cref="DateTime"/> in a protected DateTime.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected DateTime holding the given value.</returns>
        public static implicit operator ProtectedDateTime(DateTime _Value)
        {
            return new ProtectedDateTime(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected DateTime to its <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="_Value">Protected DateTime to unwrap.</param>
        /// <returns>The unobfuscated DateTime value.</returns>
        public static implicit operator DateTime(ProtectedDateTime _Value)
        {
            return _Value.Value;
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected DateTimes represent the same instant.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedDateTime v1, ProtectedDateTime v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected DateTimes represent different instants.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedDateTime v1, ProtectedDateTime v2)
        {
            return v1.Value != v2.Value;
        }

        /// <summary>
        /// Returns true when this protected DateTime equals the given object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ProtectedDateTime)
            {
                return this.Value == ((ProtectedDateTime)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        #endregion
    }
}