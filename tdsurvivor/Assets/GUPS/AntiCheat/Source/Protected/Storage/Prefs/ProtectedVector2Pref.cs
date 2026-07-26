// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Inspector-assignable wrapper around a single <see cref="Vector2"/> entry in <see cref="ProtectedPlayerPrefs"/>.
    /// </summary>
    /// <seealso cref="ProtectedIntPref"/>
    [Serializable]
    public class ProtectedVector2Pref : IProtectedPref<Vector2>
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
        private Vector2 defaultValue;

        /// <inheritdoc/>
        public Vector2 Value
        {
            get
            {
                return ProtectedPlayerPrefs.GetVector2(key, this.defaultValue);
            }
            set
            {
                ProtectedPlayerPrefs.SetVector2(key, value);
            }
        }
    }
}
