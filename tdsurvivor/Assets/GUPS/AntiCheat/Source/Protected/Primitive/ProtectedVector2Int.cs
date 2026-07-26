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
    /// Drop-in replacement for <see cref="UnityEngine.Vector2Int"/> that obfuscates each component in memory and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedVector2Int : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The XOR-obfuscated true value of the x-component.
        /// </summary>
        private Int32 obfuscatedValueX;

        /// <summary>
        /// The XOR-obfuscated true value of the y-component.
        /// </summary>
        private Int32 obfuscatedValueY;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private Int32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private Vector2Int fakeValue;

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
        /// Initializes a new protected Vector2Int with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedVector2Int(Vector2Int _Value)
        {
            this.isInitialized = true;

            this.obfuscatedValueX = 0;
            this.obfuscatedValueY = 0;

            this.secret = 0;

            this.fakeValue = Vector2Int.zero;

            this.hasIntegrity = true;

            this.Obfuscate(_Value);
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public Vector2Int Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return new Vector2Int();
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
        /// Stores the given value in XOR-obfuscated form (per component) and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(Vector2Int _Value)
        {
            this.obfuscatedValueX = (int)_Value.x ^ this.secret;
            this.obfuscatedValueY = (int)_Value.y ^ this.secret;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private Vector2Int UnObfuscate()
        {
            Vector2Int var_RealValue = new Vector2Int();

            var_RealValue.x = this.obfuscatedValueX ^ this.secret;
            var_RealValue.y = this.obfuscatedValueY ^ this.secret;

            return var_RealValue;
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            Vector2Int var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            Vector2Int var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the obfuscated components and the secret.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedValueX = 0;
            this.obfuscatedValueY = 0;
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
            return this.Value.GetHashCode();
        }

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a <see cref="Vector2Int"/> in a protected Vector2Int.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected Vector2Int holding the given value.</returns>
        public static implicit operator ProtectedVector2Int(Vector2Int _Value)
        {
            return new ProtectedVector2Int(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected Vector2Int to its <see cref="Vector2Int"/> value.
        /// </summary>
        /// <param name="_Value">Protected Vector2Int to unwrap.</param>
        /// <returns>The unobfuscated Vector2Int value.</returns>
        public static implicit operator Vector2Int(ProtectedVector2Int _Value)
        {
            return _Value.Value;
        }

        /// <summary>
        /// Implicitly converts a protected Vector2Int to a <see cref="ProtectedQuaternion"/> (zero z/w).
        /// </summary>
        /// <param name="_Value">Protected Vector2Int to convert.</param>
        /// <returns>A protected Quaternion <c>(x, y, 0, 0)</c>.</returns>
        public static implicit operator ProtectedQuaternion(ProtectedVector2Int _Value)
        {
            return new ProtectedQuaternion(new Quaternion(_Value.Value.x, _Value.Value.y, 0, 0));
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedQuaternion"/> to a protected Vector2Int (drops z/w, truncates).
        /// </summary>
        /// <param name="_Value">Protected Quaternion to convert.</param>
        /// <returns>A protected Vector2Int holding the truncated x/y components.</returns>
        public static implicit operator ProtectedVector2Int(ProtectedQuaternion _Value)
        {
            return new ProtectedVector2Int(new Vector2Int((int)_Value.Value.x, (int)_Value.Value.y));
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected vectors represent the same value.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedVector2Int v1, ProtectedVector2Int v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected vectors represent different values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedVector2Int v1, ProtectedVector2Int v2)
        {
            return v1.Value != v2.Value;
        }

        /// <summary>
        /// Returns true when this protected vector equals the given object.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ProtectedVector2Int)
            {
                return this.Value == ((ProtectedVector2Int)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        #endregion
    }
}