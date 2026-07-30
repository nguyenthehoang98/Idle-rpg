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
    /// Drop-in replacement for <see cref="System.Decimal"/> that stores the value as its raw <see cref="decimal.GetBits(decimal)"/> int array and uses a <see cref="double"/> honeypot to report tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <remarks>
    /// The honeypot is stored as a <see cref="double"/>, so integrity checks for decimal values that fall outside <see cref="double"/>'s precision range may yield false positives.
    /// </remarks>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedDecimal : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The decimal value split into its raw bit representation (output of <see cref="decimal.GetBits(decimal)"/>).
        /// </summary>
        private int[] obfuscatedValues;

        /// <summary>
        /// Honeypot value (stored as <see cref="double"/>) serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private double fakeValue;

        /// <summary>
        /// Unity serialization hook; writes the true value into the honeypot field.
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.fakeValue = (double)this.Value;
        }

        /// <summary>
        /// Unity deserialization hook; rebuilds the obfuscation state from the deserialized honeypot value.
        /// </summary>
        public void OnAfterDeserialize()
        {
            this = (decimal)this.fakeValue;
        }

        /// <summary>
        /// Initializes a new protected decimal with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedDecimal(decimal _Value = 0)
        {
            this.isInitialized = true;
            this.obfuscatedValues = decimal.GetBits(_Value);
            this.hasIntegrity = true;

            this.fakeValue = (double)_Value;
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public decimal Value
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
        /// Stores the given value as its raw bit representation and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(decimal _Value)
        {
            this.obfuscatedValues = decimal.GetBits(_Value);

            this.fakeValue = (double)_Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private decimal UnObfuscate()
        {
            return new decimal(this.obfuscatedValues);
        }

        /// <summary>
        /// Re-stores the current value (kept for interface symmetry; no secret is rerolled for the decimal variant).
        /// </summary>
        public void Obfuscate()
        {
            decimal var_UnobfuscatedValue = this.UnObfuscate();

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the stored value (compared as <see cref="double"/>).
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            decimal var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != (double)var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the stored bit array.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedValues = null;
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
        /// Implicitly wraps a decimal in a protected decimal.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected decimal holding the given value.</returns>
        public static implicit operator ProtectedDecimal(decimal _Value)
        {
            return new ProtectedDecimal(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected decimal to its decimal value.
        /// </summary>
        /// <param name="_Value">Protected decimal to unwrap.</param>
        /// <returns>The unobfuscated decimal value.</returns>
        public static implicit operator decimal(ProtectedDecimal _Value)
        {
            return _Value.Value;
        }

        /// <summary>
        /// Implicitly converts a protected decimal to a <see cref="ProtectedDouble"/>.
        /// </summary>
        /// <param name="_Value">Protected decimal to convert.</param>
        /// <returns>A protected double holding the converted value.</returns>
        public static implicit operator ProtectedDouble(ProtectedDecimal _Value)
        {
            return new ProtectedDouble((double)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedDouble"/> to a protected decimal.
        /// </summary>
        /// <param name="_Value">Protected double to convert.</param>
        /// <returns>A protected decimal holding the converted value.</returns>
        public static implicit operator ProtectedDecimal(ProtectedDouble _Value)
        {
            return new ProtectedDecimal((decimal)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected decimal to a <see cref="ProtectedInt16"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected decimal to convert.</param>
        /// <returns>A protected Int16 holding the truncated value.</returns>
        public static implicit operator ProtectedInt16(ProtectedDecimal _Value)
        {
            return new ProtectedInt16((Int16)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt16"/> to a protected decimal.
        /// </summary>
        /// <param name="_Value">Protected Int16 to convert.</param>
        /// <returns>A protected decimal holding the converted value.</returns>
        public static implicit operator ProtectedDecimal(ProtectedInt16 _Value)
        {
            return new ProtectedDecimal((decimal)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected decimal to a <see cref="ProtectedInt32"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected decimal to convert.</param>
        /// <returns>A protected Int32 holding the truncated value.</returns>
        public static implicit operator ProtectedInt32(ProtectedDecimal _Value)
        {
            return new ProtectedInt32((Int32)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt32"/> to a protected decimal.
        /// </summary>
        /// <param name="_Value">Protected Int32 to convert.</param>
        /// <returns>A protected decimal holding the converted value.</returns>
        public static implicit operator ProtectedDecimal(ProtectedInt32 _Value)
        {
            return new ProtectedDecimal((decimal)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected decimal to a <see cref="ProtectedInt64"/> (truncates).
        /// </summary>
        /// <param name="_Value">Protected decimal to convert.</param>
        /// <returns>A protected Int64 holding the truncated value.</returns>
        public static implicit operator ProtectedInt64(ProtectedDecimal _Value)
        {
            return new ProtectedInt64((Int64)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedInt64"/> to a protected decimal.
        /// </summary>
        /// <param name="_Value">Protected Int64 to convert.</param>
        /// <returns>A protected decimal holding the converted value.</returns>
        public static implicit operator ProtectedDecimal(ProtectedInt64 _Value)
        {
            return new ProtectedDecimal((decimal)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a protected decimal to a <see cref="ProtectedFloat"/> (narrows).
        /// </summary>
        /// <param name="_Value">Protected decimal to convert.</param>
        /// <returns>A protected float holding the narrowed value.</returns>
        public static implicit operator ProtectedFloat(ProtectedDecimal _Value)
        {
            return new ProtectedFloat((float)_Value.Value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedFloat"/> to a protected decimal.
        /// </summary>
        /// <param name="_Value">Protected float to convert.</param>
        /// <returns>A protected decimal holding the converted value.</returns>
        public static implicit operator ProtectedDecimal(ProtectedFloat _Value)
        {
            return new ProtectedDecimal((decimal)_Value.Value);
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected values represent the same number.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedDecimal v1, ProtectedDecimal v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected values represent different numbers.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedDecimal v1, ProtectedDecimal v2)
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
            if (obj is ProtectedDecimal)
            {
                return this.Value == ((ProtectedDecimal)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        /// <summary>
        /// Returns true when the first value is less than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <(ProtectedDecimal v1, ProtectedDecimal v2)
        {
            return v1.Value < v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is less than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &lt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator <=(ProtectedDecimal v1, ProtectedDecimal v2)
        {
            return v1.Value <= v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt; <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >(ProtectedDecimal v1, ProtectedDecimal v2)
        {
            return v1.Value > v2.Value;
        }

        /// <summary>
        /// Returns true when the first value is greater than or equal to the second.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if <paramref name="v1"/> &gt;= <paramref name="v2"/>; otherwise <c>false</c>.</returns>
        public static bool operator >=(ProtectedDecimal v1, ProtectedDecimal v2)
        {
            return v1.Value >= v2.Value;
        }

        #endregion
    }
}