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
    /// Drop-in replacement for <see cref="System.Single"/> that obfuscates the value in memory (via a <see cref="UIntFloat"/> reinterpret cast) and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedFloat : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The XOR-obfuscated true value (reinterpret-cast as a uint).
        /// </summary>
        private UIntFloat obfuscatedValue;

        /// <summary>
        /// Scratch union used to reinterpret-cast between <see cref="float"/> and <see cref="uint"/> during (un)obfuscation without going through a lossy numeric conversion.
        /// </summary>
        private UIntFloat manager;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private UInt32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private float fakeValue;

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
        /// Initializes a new protected float with the specified value.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public ProtectedFloat(float value = 0)
        {
            this.isInitialized = true;
            this.secret = (UInt32)GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.obfuscatedValue.intValue = 0;
            this.obfuscatedValue.floatValue = value;
            this.obfuscatedValue.intValue = this.obfuscatedValue.intValue ^ this.secret;

            this.manager.intValue = 0;
            this.manager.floatValue = 0;

            this.hasIntegrity = true;

            this.fakeValue = value;
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public float Value
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
        private void Obfuscate(float _Value)
        {
            this.manager.floatValue = _Value;
            this.manager.intValue = this.manager.intValue ^ this.secret;
            this.obfuscatedValue.floatValue = this.manager.floatValue;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private float UnObfuscate()
        {
            this.manager.intValue = this.obfuscatedValue.intValue ^ this.secret;

            return this.manager.floatValue;
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            float var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = (UInt32)GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            float var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the obfuscated value, scratch union and secret.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedValue.intValue = 0;
            this.manager.intValue = 0;
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
        /// Returns the hash code of the obfuscated bit pattern.
        /// </summary>
        /// <returns>Hash code based on the stored obfuscated bits (does not unobfuscate).</returns>
        public override int GetHashCode()
        {
            return this.obfuscatedValue.floatValue.GetHashCode();
        }

        #region Serialization

        /// <summary>
        /// Writes the obfuscated value and secret to the given out parameters for player-prefs storage.
        /// </summary>
        /// <param name="_ObfuscatedValue">Receives the obfuscated bit pattern (as uint).</param>
        /// <param name="_Secret">Receives the secret key.</param>
        internal void Serialize(out uint _ObfuscatedValue, out uint _Secret)
        {
            _ObfuscatedValue = this.obfuscatedValue.intValue;
            _Secret = this.secret;
        }

        /// <summary>
        /// Restores the obfuscated value and secret from the given parameters.
        /// </summary>
        /// <param name="_ObfuscatedValue">Previously stored obfuscated bit pattern.</param>
        /// <param name="_Secret">Previously stored secret key.</param>
        internal void Deserialize(uint _ObfuscatedValue, uint _Secret)
        {
            this.obfuscatedValue.intValue = _ObfuscatedValue;
            this.secret = _Secret;
            this.fakeValue = this.UnObfuscate();
        }

        #endregion

        #region Implicit operators

        /// <summary>
        /// Implicitly wraps a float in a protected float.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected float holding the given value.</returns>
        public static implicit operator ProtectedFloat(float _Value)
        {
            return new ProtectedFloat(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected float to its float value.
        /// </summary>
        /// <param name="_Value">Protected float to unwrap.</param>
        /// <returns>The unobfuscated float value.</returns>
        public static implicit operator float(ProtectedFloat _Value)
        {
            return _Value.Value;
        }

        /// <summary>
        /// Implicitly converts a protected float to a <see cref="ProtectedInt16"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected float to convert.</param>
        /// <returns>A protected Int16 holding the truncated value.</returns>
        public static implicit operator ProtectedInt16(ProtectedFloat _Value)
        {
            return new ProtectedInt16((Int16)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt16"/> to a protected float.
        /// </summary>
        /// <param name="_Value">Protected Int16 to convert.</param>
        /// <returns>A protected float holding the converted value.</returns>
        public static implicit operator ProtectedFloat(ProtectedInt16 _Value)
        {
            return new ProtectedFloat((float)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected float to a <see cref="ProtectedInt32"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected float to convert.</param>
        /// <returns>A protected Int32 holding the truncated value.</returns>
        public static implicit operator ProtectedInt32(ProtectedFloat _Value)
        {
            return new ProtectedInt32((Int32)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt32"/> to a protected float.
        /// </summary>
        /// <param name="_Value">Protected Int32 to convert.</param>
        /// <returns>A protected float holding the converted value.</returns>
        public static implicit operator ProtectedFloat(ProtectedInt32 _Value)
        {
            return new ProtectedFloat((float)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected float to a <see cref="ProtectedInt64"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected float to convert.</param>
        /// <returns>A protected Int64 holding the truncated value.</returns>
        public static implicit operator ProtectedInt64(ProtectedFloat _Value)
        {
            return new ProtectedInt64((Int64)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt64"/> to a protected float.
        /// </summary>
        /// <param name="_Value">Protected Int64 to convert.</param>
        /// <returns>A protected float holding the converted value.</returns>
        public static implicit operator ProtectedFloat(ProtectedInt64 _Value)
        {
            return new ProtectedFloat((float)_Value.Value);
        }

        #endregion

        #region Calculation operators

        /// <summary>
        /// Adds two protected values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The sum.</returns>
        public static ProtectedFloat operator +(ProtectedFloat v1, ProtectedFloat v2)
        {
            return new ProtectedFloat(v1.Value + v2.Value);
        }

        /// <summary>
        /// Subtracts the second protected value from the first.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The difference.</returns>
        public static ProtectedFloat operator -(ProtectedFloat v1, ProtectedFloat v2)
        {
            return new ProtectedFloat(v1.Value - v2.Value);
        }

        /// <summary>
        /// Multiplies two protected values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The product.</returns>
        public static ProtectedFloat operator *(ProtectedFloat v1, ProtectedFloat v2)
        {
            return new ProtectedFloat(v1.Value * v2.Value);
        }

        /// <summary>
        /// Divides the first protected value by the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns>The quotient.</returns>
        public static ProtectedFloat operator /(ProtectedFloat v1, ProtectedFloat v2)
        {
            return new ProtectedFloat(v1.Value / v2.Value);
        }

        #endregion

        #region Equality operators

        /// <summary>
        /// Returns true when both protected values represent the same number.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Uses direct <see cref="float"/> equality and is therefore subject to the usual floating-point precision caveats.
        /// </remarks>
        public static bool operator ==(ProtectedFloat v1, ProtectedFloat v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected values represent different numbers.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedFloat v1, ProtectedFloat v2)
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
            if (obj is ProtectedFloat)
            {
                return this.Value == ((ProtectedFloat)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        /// <summary>
        /// Returns true when the first value is less than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <(ProtectedFloat v1, ProtectedFloat v2)
        {
            return v1.Value < v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is less than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <=(ProtectedFloat v1, ProtectedFloat v2)
        {
            return v1.Value <= v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >(ProtectedFloat v1, ProtectedFloat v2)
        {
            return v1.Value > v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >=(ProtectedFloat v1, ProtectedFloat v2)
        {
            return v1.Value >= v2.Value;
        }

        #endregion
    }
}