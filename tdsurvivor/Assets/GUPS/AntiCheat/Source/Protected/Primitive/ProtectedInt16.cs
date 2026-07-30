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
    /// Drop-in replacement for <see cref="System.Int16"/> that obfuscates the value in memory and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedInt16 : IProtected, IDisposable, ISerializationCallbackReceiver
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
        private Int16 obfuscatedValue;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private Int16 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private Int16 fakeValue;

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
        /// Initializes a new protected Int16 with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedInt16(Int16 _Value = 0)
        {
            this.isInitialized = true;
            this.obfuscatedValue = 0;
            this.secret = (Int16)GlobalSettings.RandomProvider.RandomInt32(1, Int16.MaxValue);
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
        public Int16 Value
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
        private void Obfuscate(Int16 _Value)
        {
            this.obfuscatedValue = (Int16)(_Value ^ this.secret);

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private Int16 UnObfuscate()
        {
            return (Int16)(this.obfuscatedValue ^ this.secret);
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            Int16 var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = (Int16)GlobalSettings.RandomProvider.RandomInt32(1, Int16.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            Int16 var_UnobfuscatedValue = this.UnObfuscate();

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
        /// Implicitly wraps an Int16 in a protected Int16.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected Int16 holding the given value.</returns>
        public static implicit operator ProtectedInt16(Int16 _Value)
        {
            return new ProtectedInt16(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected Int16 to its Int16 value.
        /// </summary>
        /// <param name="_Value">Protected Int16 to unwrap.</param>
        /// <returns>The unobfuscated Int16 value.</returns>
        public static implicit operator Int16(ProtectedInt16 _Value)
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
        public static ProtectedInt16 operator +(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return new ProtectedInt16((Int16)(v1.Value + v2.Value));
        }

        /// <summary>
        /// Subtracts the second protected value from the first.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The difference.</returns>
        public static ProtectedInt16 operator -(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return new ProtectedInt16((Int16)(v1.Value - v2.Value));
        }

        /// <summary>
        /// Multiplies two protected values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The product.</returns>
        public static ProtectedInt16 operator *(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return new ProtectedInt16((Int16)(v1.Value * v2.Value));
        }

        /// <summary>
        /// Divides the first protected value by the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The quotient.</returns>
        public static ProtectedInt16 operator /(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return new ProtectedInt16((Int16)(v1.Value / v2.Value));
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected values represent the same number.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected values represent different numbers.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedInt16 v1, ProtectedInt16 v2)
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
            if (obj is ProtectedInt16)
            {
                return this.Value == ((ProtectedInt16)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        /// <summary>
        /// Returns true when the first value is less than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return v1.Value < v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is less than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <=(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return v1.Value <= v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return v1.Value > v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >=(ProtectedInt16 v1, ProtectedInt16 v2)
        {
            return v1.Value >= v2.Value;
        }

        #endregion
    }
}