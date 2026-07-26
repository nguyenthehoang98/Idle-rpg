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
    /// Drop-in replacement for <see cref="UnityEngine.Vector3"/> that obfuscates each component in memory (via <see cref="UIntFloat"/> reinterpret casts) and reports tampering to the <see cref="PrimitiveCheatingDetector"/>.
    /// </summary>
    /// <seealso cref="ProtectedInt32"/>
    [Serializable]
    public struct ProtectedVector3 : IProtected, IDisposable, ISerializationCallbackReceiver
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
        /// The XOR-obfuscated true value of the x-component (reinterpret-cast as a uint).
        /// </summary>
        private UIntFloat obfuscatedValueX;

        /// <summary>
        /// The XOR-obfuscated true value of the y-component (reinterpret-cast as a uint).
        /// </summary>
        private UIntFloat obfuscatedValueY;

        /// <summary>
        /// The XOR-obfuscated true value of the z-component (reinterpret-cast as a uint).
        /// </summary>
        private UIntFloat obfuscatedValueZ;

        /// <summary>
        /// Scratch union used to reinterpret-cast between <see cref="float"/> and <see cref="uint"/> during per-component (un)obfuscation.
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
        private Vector3 fakeValue;

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
        /// Initializes a new protected Vector3 with the specified value.
        /// </summary>
        /// <param name="_Value">Initial value.</param>
        public ProtectedVector3(Vector3 _Value)
        {
            this.isInitialized = true;

            this.obfuscatedValueX.intValue = 0;
            this.obfuscatedValueX.floatValue = 0;
            this.obfuscatedValueY.intValue = 0;
            this.obfuscatedValueY.floatValue = 0;
            this.obfuscatedValueZ.intValue = 0;
            this.obfuscatedValueZ.floatValue = 0;

            this.manager.intValue = 0;
            this.manager.floatValue = 0;

            this.secret = 0;

            this.fakeValue = Vector3.zero;

            this.hasIntegrity = true;

            this.Obfuscate(_Value);
        }

        /// <summary>
        /// Gets or sets the unobfuscated value.
        /// </summary>
        /// <remarks>
        /// Reading the value runs an integrity check; if the honeypot has been tampered with, the <see cref="PrimitiveCheatingDetector"/> is notified.
        /// </remarks>
        public Vector3 Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    return new Vector3();
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
        private void Obfuscate(Vector3 _Value)
        {
            this.manager.floatValue = _Value.x;
            this.manager.intValue = this.manager.intValue ^ this.secret;
            this.obfuscatedValueX.floatValue = this.manager.floatValue;

            this.manager.floatValue = _Value.y;
            this.manager.intValue = this.manager.intValue ^ this.secret;
            this.obfuscatedValueY.floatValue = this.manager.floatValue;

            this.manager.floatValue = _Value.z;
            this.manager.intValue = this.manager.intValue ^ this.secret;
            this.obfuscatedValueZ.floatValue = this.manager.floatValue;

            this.fakeValue = _Value;
        }

        /// <summary>
        /// Returns the unobfuscated true value.
        /// </summary>
        /// <returns>The unobfuscated value.</returns>
        private Vector3 UnObfuscate()
        {
            Vector3 var_RealValue = new Vector3();

            this.manager.intValue = this.obfuscatedValueX.intValue ^ this.secret;
            var_RealValue.x = this.manager.floatValue;

            this.manager.intValue = this.obfuscatedValueY.intValue ^ this.secret;
            var_RealValue.y = this.manager.floatValue;

            this.manager.intValue = this.obfuscatedValueZ.intValue ^ this.secret;
            var_RealValue.z = this.manager.floatValue;

            return var_RealValue;
        }

        /// <summary>
        /// Rerolls the secret and re-obfuscates the current value.
        /// </summary>
        public void Obfuscate()
        {
            Vector3 var_UnobfuscatedValue = this.UnObfuscate();

            this.secret = (UInt32)GlobalSettings.RandomProvider.RandomInt32(1, Int32.MaxValue);

            this.Obfuscate(var_UnobfuscatedValue);
        }

        /// <summary>
        /// Returns true when the honeypot still matches the obfuscated value.
        /// </summary>
        /// <returns><c>true</c> if the value is intact; otherwise <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            Vector3 var_UnobfuscatedValue = this.UnObfuscate();

            if (this.fakeValue != var_UnobfuscatedValue)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Clears the obfuscated components, scratch union and secret.
        /// </summary>
        public void Dispose()
        {
            this.obfuscatedValueX.intValue = 0;
            this.obfuscatedValueY.intValue = 0;
            this.obfuscatedValueZ.intValue = 0;
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
        /// Returns the hash code of the unobfuscated value.
        /// </summary>
        /// <returns>Hash code of the unobfuscated value.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        #region Serialization

        /// <summary>
        /// Writes the obfuscated components and secret to the given out parameters for player-prefs storage.
        /// </summary>
        /// <param name="_ObfuscatedValueX">Receives the obfuscated x bit pattern.</param>
        /// <param name="_ObfuscatedValueY">Receives the obfuscated y bit pattern.</param>
        /// <param name="_ObfuscatedValueZ">Receives the obfuscated z bit pattern.</param>
        /// <param name="_Secret">Receives the secret key.</param>
        internal void Serialize(out UInt32 _ObfuscatedValueX, out UInt32 _ObfuscatedValueY, out UInt32 _ObfuscatedValueZ, out UInt32 _Secret)
        {
            _ObfuscatedValueX = this.obfuscatedValueX.intValue;
            _ObfuscatedValueY = this.obfuscatedValueY.intValue;
            _ObfuscatedValueZ = this.obfuscatedValueZ.intValue;
            _Secret = this.secret;
        }

        /// <summary>
        /// Restores the obfuscated components and secret from the given parameters.
        /// </summary>
        /// <param name="_ObfuscatedValueX">Previously stored obfuscated x bit pattern.</param>
        /// <param name="_ObfuscatedValueY">Previously stored obfuscated y bit pattern.</param>
        /// <param name="_ObfuscatedValueZ">Previously stored obfuscated z bit pattern.</param>
        /// <param name="_Secret">Previously stored secret key.</param>
        internal void Deserialize(UInt32 _ObfuscatedValueX, UInt32 _ObfuscatedValueY, UInt32 _ObfuscatedValueZ, UInt32 _Secret)
        {
            this.obfuscatedValueX.intValue = _ObfuscatedValueX;
            this.obfuscatedValueY.intValue = _ObfuscatedValueY;
            this.obfuscatedValueZ.intValue = _ObfuscatedValueZ;
            this.secret = _Secret;
            this.fakeValue = this.UnObfuscate();
        }

        #endregion

        #region Implicit operator

        /// <summary>
        /// Implicitly wraps a <see cref="Vector3"/> in a protected Vector3.
        /// </summary>
        /// <param name="_Value">Value to wrap.</param>
        /// <returns>A protected Vector3 holding the given value.</returns>
        public static implicit operator ProtectedVector3(Vector3 _Value)
        {
            return new ProtectedVector3(_Value);
        }

        /// <summary>
        /// Implicitly unwraps a protected Vector3 to its <see cref="Vector3"/> value.
        /// </summary>
        /// <param name="_Value">Protected Vector3 to unwrap.</param>
        /// <returns>The unobfuscated Vector3 value.</returns>
        public static implicit operator Vector3(ProtectedVector3 _Value)
        {
            return _Value.Value;
        }

        /// <summary>
        /// Implicitly converts a protected Vector3 to a <see cref="ProtectedQuaternion"/> (zero w).
        /// </summary>
        /// <param name="_Value">Protected Vector3 to convert.</param>
        /// <returns>A protected Quaternion <c>(x, y, z, 0)</c>.</returns>
        public static implicit operator ProtectedQuaternion(ProtectedVector3 _Value)
        {
            return new ProtectedQuaternion(new Quaternion(_Value.Value.x, _Value.Value.y, _Value.Value.z, 0));
        }

        /// <summary>
        /// Implicitly converts a <see cref="ProtectedQuaternion"/> to a protected Vector3 (drops w).
        /// </summary>
        /// <param name="_Value">Protected Quaternion to convert.</param>
        /// <returns>A protected Vector3 containing only the x/y/z components.</returns>
        public static implicit operator ProtectedVector3(ProtectedQuaternion _Value)
        {
            return new ProtectedVector3(new Vector3(_Value.Value.x, _Value.Value.y, _Value.Value.z));
        }

        #endregion

        #region Equality operator

        /// <summary>
        /// Returns true when both protected vectors represent the same value.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ProtectedVector3 v1, ProtectedVector3 v2)
        {
            return v1.Value == v2.Value;
        }

        /// <summary>
        /// Returns true when the two protected vectors represent different values.
        /// </summary>
        /// <param name="v1">First operand.</param>
        /// <param name="v2">Second operand.</param>
        /// <returns><c>true</c> if not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ProtectedVector3 v1, ProtectedVector3 v2)
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
            if (obj is ProtectedVector3)
            {
                return this.Value == ((ProtectedVector3)obj).Value;
            }
            return this.Value.Equals(obj);
        }

        #endregion
    }
}