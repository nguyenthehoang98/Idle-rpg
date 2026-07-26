// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Inspector-assignable wrapper around a single <see cref="Vector3"/> entry in <see cref="ProtectedPlayerPrefs"/>.
    /// </summary>
    /// <seealso cref="ProtectedIntPref"/>
    [Serializable]
    public class ProtectedVector3Pref : IProtectedPref<Vector3>
    {
        /// <summary>
        /// The serialized key under which the value is stored in <see cref="ProtectedPlayerPrefs"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The unique key associated with the player preference.")]
        private string key;

        /// <inheritdoc/>
        public String Key => this.Key;

        /// <summary>
        /// The fallback value returned when no entry exists for <see cref="Key"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The default value if the player preference is not set.")]
        private Vector3 defaultValue;

        /// <inheritdoc/>
        public Vector3 Value
        {
            get
            {
                return ProtectedPlayerPrefs.GetVector3(key, this.defaultValue);
            }
            set
            {
                ProtectedPlayerPrefs.SetVector3(key, value);
            }
        }
    }
}
