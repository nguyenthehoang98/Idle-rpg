// System
using System;
using System.Reflection;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Punisher;

namespace GUPS.AntiCheat.Punisher
{
    /// <summary>
    /// Drastic punisher that closes the application via <see cref="Application.Quit()"/> once the configured
    /// threat rating is reached.
    /// </summary>
    /// <example>
    /// Attach the punisher as a child of the <see cref="GUPS.AntiCheat.AntiCheatMonitor"/> GameObject so it is
    /// auto-registered on <c>Awake</c>. The default <c>threatRating</c> is <c>850</c>; tweak it in the inspector
    /// or by overriding the serialized field in a prefab variant:
    /// <code>
    /// using GUPS.AntiCheat;
    /// using GUPS.AntiCheat.Punisher;
    /// using UnityEngine;
    ///
    /// var monitor = AntiCheatMonitor.Instance;
    ///
    /// var host = new GameObject("ExitGamePunisher");
    /// host.transform.SetParent(monitor.transform);
    /// host.AddComponent&lt;ExitGamePunisher&gt;();
    /// </code>
    /// </example>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class ExitGamePunisher : MonoBehaviour, IPunisher
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public String Name => "Exit Game Punisher";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
        public bool IsSupported => true;

        /// <summary>
        /// Backing field for <see cref="IsActive"/>.
        /// </summary>
        [SerializeField]
        [Header("Punisher - Settings")]
        [Tooltip("Gets or sets whether the punisher is active and can administer punitive actions (Default: true).")]
        private bool isActive = true;

        /// <inheritdoc/>
        public bool IsActive { get => this.isActive; set => this.isActive = value; }

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Backing field for <see cref="ThreatRating"/>. Defaults to <c>850</c> to reflect how drastic quitting is.
        /// </summary>
        [SerializeField]
        [Tooltip("Is a very drastic punishment, so the threat rating is set to a high value (Default: 850).")]
        private uint threatRating = 850;

        /// <inheritdoc/>
        public uint ThreatRating => this.threatRating;

        #endregion

        // Punishment
        #region Punishment

        /// <inheritdoc/>
        public bool PunishOnce => true;

        /// <inheritdoc/>
        public bool HasPunished { get; private set; } = false;

        /// <summary>
        /// Quits the application via <see cref="Application.Quit()"/> and flags <see cref="HasPunished"/>.
        /// </summary>
        public void Punish()
        {
            // Flag first so any observer watching HasPunished can react before the process is torn down by Quit().
            this.HasPunished = true;

            Application.Quit();
        }

        #endregion
    }
}
