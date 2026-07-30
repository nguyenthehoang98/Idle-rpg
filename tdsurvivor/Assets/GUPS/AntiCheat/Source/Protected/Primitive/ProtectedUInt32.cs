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
    /// Drop-in replacement for <see cref="System.UInt32"/> that obfuscates the value in memory and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedUInt32 : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The XOR-obfuscated true value.
        /// </summary>
        private UInt32 obfuscatedValue;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private UInt32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private UInt32 fakeValue;

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
        /// Initializes a new protected UInt32 with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedUInt32(UInt32 _Value = 0)
        {
            this.isInitialized = true;
            this.obfuscatedValue = 0;
            this.secret = (UInt32)GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);
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
        public UInt32 Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return 0;
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
        /// Stores the given value in XOR-obfuscated form and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(UInt32 _Value)
        {
            this.obfuscatedValue = _Value ^ this.secret;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private UInt32 UnObfuscate()
        {
            return (UInt32)(this.obfuscatedValue ^ this.secret);
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            UInt32 var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = (UInt32)GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            UInt32 var_UnobfuscatedValue = this.UnObfuscate();

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
            return (int)this.Value;
        }

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a UInt32 in a protected UInt32.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected UInt32 holding the given value.</returns>
        public static implicit operator ProtectedUInt32(UInt32 _Value)
        {
            return new ProtectedUInt32(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected UInt32 to its UInt32 value.
        /// </summary>
        /// <param name="_Value">Protected UInt32 to unwrap.</param>
        /// <returns>The unobfuscated UInt32 value.</returns>
        public static implicit operator UInt32(ProtectedUInt32 _Value)
        {
            return _Value.Value;
        }

        #endregion

        #region Calculation operator

        /// <summary>
        /// Adds two protected values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The sum.</returns>
        public static ProtectedUInt32 operator +(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return new ProtectedUInt32(v1.Value + v2.Value);
        }

        /// <summary>
        /// Subtracts the second protected value from the first.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The difference.</returns>
        public static ProtectedUInt32 operator -(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return new ProtectedUInt32(v1.Value - v2.Value);
        }

        /// <summary>
        /// Multiplies two protected values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The product.</returns>
        public static ProtectedUInt32 operator *(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return new ProtectedUInt32(v1.Value * v2.Value);
        }

        /// <summary>
        /// Divides the first protected value by the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The quotient.</returns>
        public static ProtectedUInt32 operator /(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return new ProtectedUInt32(v1.Value / v2.Value);
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected values represent the same number.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected values represent different numbers.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value != v2.Value;
        }

        /// <summary>
        /// Returns true when this protected value equals the given object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ProtectedUInt32)
            {
                return this.Value == ((ProtectedUInt32)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        /// <summary>
        /// Returns true when the first value is less than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value < v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is less than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <=(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value <= v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value > v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >=(ProtectedUInt32 v1, ProtectedUInt32 v2)
        {
            return v1.Value >= v2.Value;
        }

        #endregion
    }
}