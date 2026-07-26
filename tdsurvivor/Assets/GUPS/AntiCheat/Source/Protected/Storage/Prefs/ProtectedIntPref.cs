// System
using System;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Inspector-assignable wrapper around a single <see cref="Int32"/> entry in <see cref="ProtectedPlayerPrefs"/>.
    /// </summary>
    /// <example>
    /// Set, get and persist a tamper-resistant integer preference:
    /// <code>
    /// using GUPS.AntiCheat.Protected.Storage.Prefs;
    ///
    /// ProtectedPlayerPrefs.SetInt("HighScore", 1337);
    /// int score = ProtectedPlayerPrefs.GetInt("HighScore", _DefaultValue: 0);
    /// ProtectedPlayerPrefs.Save();
    /// </code>
    /// The stored value is XOR-encrypted with the configured secret and bound to
    /// <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/> when the
    /// "Allow Read Any Owner" setting is disabled.
    /// </example>
    /// <seealso cref="ProtectedPlayerPrefs"/>
    [Serializable]
    public class ProtectedIntPref : IProtectedPref<Int32>
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
        private Int32 defaultValue;

        /// <inheritdoc/>
        public Int32 Value
        {
            get
            {
                return ProtectedPlayerPrefs.GetInt(key, this.defaultValue);
            }
            set
            {
                ProtectedPlayerPrefs.SetInt(key, value);
            }
        }
    }
}
