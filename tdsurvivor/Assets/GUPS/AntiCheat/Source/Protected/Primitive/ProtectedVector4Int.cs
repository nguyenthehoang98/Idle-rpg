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
    /// Drop-in replacement for an integer-component Vector4 that obfuscates each component in memory and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <remarks>
    /// Unity has no built-in <c>Vector4Int</c>, so this type stores the honeypot as a <see cref="Vector4"/> while the four components are obfuscated as <see cref="Int32"/>; non-integral inputs are truncated.
    /// </remarks>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedVector4Int : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The XOR-obfuscated true value of the z-component.
        /// </summary>
        private Int32 obfuscatedValueZ;

        /// <summary>
        /// The XOR-obfuscated true value of the w-component.
        /// </summary>
        private Int32 obfuscatedValueW;

        /// <summary>
        /// Random secret used to obfuscate / de-obfuscate the true value.
        /// </summary>
        private Int32 secret;

        /// <summary>
        /// Honeypot value serialized in place of the true value; tampering with it triggers the primitive cheating detector.
        /// </summary>
        [SerializeField]
        private Vector4 fakeValue;

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
        /// Initializes a new protected Vector4Int with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value; non-integral components are truncated.</param>
        public ProtectedVector4Int(Vector4 _Value)
        {
            this.isInitialized = true;

            this.obfuscatedValueX = 0;
            this.obfuscatedValueY = 0;
            this.obfuscatedValueZ = 0;
            this.obfuscatedValueW = 0;

            this.secret = 0;

            this.fakeValue = Vector4.zero;

            this.hasIntegrity = true;

            this.Obfuscate(_Value);
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public Vector4 Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return new Vector4();
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
        /// Stores the given value in XOR-obfuscated form (per component, truncated to integer) and updates the honeypot.
        /// </summary>
        /// <param name="_Value">Value to obfuscate.</param>
        private void Obfuscate(Vector4 _Value)
        {
            this.obfuscatedValueX = (int)_Value.x ^ this.secret;
            this.obfuscatedValueY = (int)_Value.y ^ this.secret;
            this.obfuscatedValueZ = (int)_Value.z ^ this.secret;
            this.obfuscatedValueW = (int)_Value.w ^ this.secret;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private Vector4 UnObfuscate()
        {
            Vector4 var_RealValue = new Vector4();

            var_RealValue.x = this.obfuscatedValueX ^ this.secret;
            var_RealValue.y = this.obfuscatedValueY ^ this.secret;
            var_RealValue.z = this.obfuscatedValueZ ^ this.secret;
            var_RealValue.w = this.obfuscatedValueW ^ this.secret;

            return var_RealValue;
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            Vector4 var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            Vector4 var_UnobfuscatedValue = this.UnObfuscate();

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
            this.obfuscatedValueZ = 0;
            this.obfuscatedValueW = 0;
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
        /// Implicitly wraps a <see cref="Vector4"/> in a protected Vector4Int (components are truncated to integers).
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected Vector4Int holding the truncated value.</returns>
        public static implicit operator ProtectedVector4Int(Vector4 _Value)
        {
            return new ProtectedVector4Int(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected Vector4Int to its <see cref="Vector4"/> value.
        /// </summary>
        /// <param name="_Value">Protected Vector4Int to unwrap.</param>
        /// <returns>The unobfuscated Vector4 value.</returns>
        public static implicit operator Vector4(ProtectedVector4Int _Value)
        {
            return _Value.Value;
        }

        /// <summary>
        /// Implicitly converts a protected Vector4Int to a <see cref="ProtectedQuaternion"/>.
        /// </summary>
        /// <param name="_Value">Protected Vector4Int to convert.</param>
        /// <returns>A protected Quaternion built from the x/y/z/w components.</returns>
        public static implicit operator ProtectedQuaternion(ProtectedVector4Int _Value)
        {
            return new ProtectedQuaternion(new Quaternion(_Value.Value.x, _Value.Value.y, _Value.Value.z, _Value.Value.w));
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedQuaternion"/> to a protected Vector4Int (components are truncated to integers).
        /// </summary>
        /// <param name="_Value">Protected Quaternion to convert.</param>
        /// <returns>A protected Vector4Int holding the truncated x/y/z/w components.</returns>
        public static implicit operator ProtectedVector4Int(ProtectedQuaternion _Value)
        {
            return new ProtectedVector4Int(new Vector4(_Value.Value.x, _Value.Value.y, _Value.Value.z, _Value.Value.w));
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected vectors represent the same value.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedVector4Int v1, ProtectedVector4Int v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected vectors represent different values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedVector4Int v1, ProtectedVector4Int v2)
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
            if (obj is ProtectedVector4Int)
            {
                return this.Value == ((ProtectedVector4Int)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        #endregion
    }
}
