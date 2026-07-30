// System
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEngine;

// GUPS - AnitCheat - Core
using GUPS.AntiCheat.Core.Detector;
using GUPS.AntiCheat.Core.Punisher;
using GUPS.AntiCheat.Core.Watch;

// GUPS - AnitCheat
using GUPS.AntiCheat.Singleton;

namespace GUPS.AntiCheat
{
    /// <summary>
    /// Central anti-cheat coordinator: aggregates threats from all <see cref="IDetector"/> children and dispatches
    /// to all <see cref="IPunisher"/> children. Implemented as a persistent singleton.
    /// </summary>
    /// <remarks>
    /// On <c>Awake</c> the monitor discovers detectors and punishers under its <see cref="GameObject"/>, subscribes to
    /// each supported detector and sorts the punishers by ascending <see cref="IThreatRated.ThreatRating"/>. Incoming
    /// detection events raise the internal threat level (scaled by <see cref="SensitiveLevel"/> and the reported
    /// false-positive chance), which slowly decays during <c>FixedUpdate</c>.
    /// </remarks>
    /// <example>
    /// Place a single <c>AntiCheatMonitor</c> in your bootstrap scene; it persists across scene loads and exposes
    /// detectors / punishers via <see cref="GetDetector{TDetector}"/> / <see cref="GetPunisher(uint)"/>:
    /// <code>
    /// using GUPS.AntiCheat;
    /// using GUPS.AntiCheat.Detector;
    /// using GUPS.AntiCheat.Punisher;
    /// using UnityEngine;
    ///
    /// var monitor = AntiCheatMonitor.Instance;
    ///
    /// // React to primitive-tampering events via the detector's UnityEvent.
    /// var primitiveDetector = monitor.GetDetector&lt;PrimitiveCheatingDetector&gt;();
    /// primitiveDetector?.OnCheatingDetectionEvent.AddListener(
    ///     status =&gt; Debug.LogWarning($"Cheat detected (rating={status.ThreatRating})."));
    ///
    /// // Attach a punisher as a child of the monitor's GameObject so it is auto-registered.
    /// var punisherHost = new GameObject("ExitGamePunisher");
    /// punisherHost.transform.SetParent(monitor.transform);
    /// punisherHost.AddComponent&lt;ExitGamePunisher&gt;();
    /// </code>
    /// See <see cref="GUPS.AntiCheat.Settings.GlobalSettings"/> for project-wide configuration.
    /// </example>
    public class AntiCheatMonitor : Singleton<AntiCheatMonitor>, IWatcher<IDetectorStatus>
    {
        // Singleton
        #region Singleton

        /// <inheritdoc/>
        public override bool IsPersistent => true;

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Backing field for <see cref="SensitiveLevel"/>.
        /// </summary>
        [SerializeField]
        [Tooltip("The sensitive level of the monitor. Manage the reaction sensitivity of the monitor to possible detected threats. Ratings of detected threats are scaled by the sensitive level beginning at 0 (NOT_SENSITIVE) up to 4 (VERY_SENSITIVE). The higher the sensitive level, the earlier the monitor will react and punish the cheater.")]
        private ESensitiveLevel sensitiveLevel = ESensitiveLevel.MODERATE;

        /// <summary>
        /// Gets the reaction sensitivity used to scale incoming threat ratings, from
        /// <see cref="ESensitiveLevel.NOT_SENSITIVE"/> (0) to <see cref="ESensitiveLevel.VERY_SENSITIVE"/> (4).
        /// </summary>
        public ESensitiveLevel SensitiveLevel { get => this.sensitiveLevel; }

        /// <summary>
        /// Current accumulated threat level.
        /// </summary>
        private float threatLevel = 0.0f;

        /// <summary>
        /// Gets the current accumulated threat level. Decays over time in <c>FixedUpdate</c>.
        /// </summary>
        public float ThreatLevel => this.threatLevel;

        #endregion

        // Detector
        #region Detector

        /// <summary>
        /// All detectors the monitor is currently subscribed to.
        /// </summary>
        private List<IDetector> detectors = new List<IDetector>();

        /// <summary>
        /// Registers a detector and subscribes the monitor to its notifications.
        /// </summary>
        /// <param name="_Detector">The detector to register.</param>
        private void RegisterDetector(IDetector _Detector)
        {
            if (!this.detectors.Contains(_Detector))
            {
                this.detectors.Add(_Detector);

                _Detector.Subscribe(this);
            }
        }

        /// <summary>
        /// Returns the first registered detector assignable to <typeparamref name="TDetector"/>, or
        /// <c>default</c> if none is found.
        /// </summary>
        /// <typeparam name="TDetector">Detector type to look up.</typeparam>
        /// <returns>The matching detector, or <c>default</c>.</returns>
        public TDetector GetDetector<TDetector>() 
            where TDetector : IDetector
        {
            foreach (var var_Detector in this.detectors)
            {
                if (var_Detector is TDetector var_TypedDetector)
                {
                    return var_TypedDetector;
                }
            }

            return default;
        }

        /// <summary>
        /// Removes a previously registered detector.
        /// </summary>
        /// <param name="_Detector">The detector to remove.</param>
        private void RemoveDetector(IDetector _Detector)
        {
            if (this.detectors.Contains(_Detector))
            {
                this.detectors.Remove(_Detector);
            }
        }

        /// <summary>
        /// Receives a threat status from a subscribed detector, updates <see cref="ThreatLevel"/> and triggers
        /// every eligible punisher.
        /// </summary>
        /// <param name="value">The reported detector status.</param>
        public void OnNext(IDetectorStatus value)
        {
            // Scale the rating by the inverse false-positive chance and the sensitivity level.
            this.threatLevel += (1f - value.PossibilityOfFalsePositive) * value.ThreatRating * (int)this.SensitiveLevel;

            var var_Punisher = this.GetPunisher((UInt32)this.threatLevel);

            var_Punisher.ForEach(p => p.Punish());
        }

        /// <summary>
        /// Notification from a detector that an error occurred. No-op.
        /// </summary>
        /// <param name="_Error">The error that occurred.</param>
        public void OnError(Exception _Error)
        {
            // Do nothing.
        }

        /// <summary>
        /// Notification from a detector that it completed. No-op.
        /// </summary>
        public void OnCompleted()
        {
            // Do nothing.
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            // Do nothing.
        }

        #endregion

        // Punisher
        #region Punisher

        /// <summary>
        /// All punishers known to the monitor, sorted by ascending threat rating.
        /// </summary>
        private List<IPunisher> punishers = new List<IPunisher>();

        /// <summary>
        /// Registers a punisher with the monitor.
        /// </summary>
        /// <param name="_Punisher">The punisher to register.</param>
        private void RegisterPunisher(IPunisher _Punisher)
        {
            if (!this.punishers.Contains(_Punisher))
            {
                this.punishers.Add(_Punisher);
            }
        }

        /// <summary>
        /// Returns all active punishers whose <see cref="IThreatRated.ThreatRating"/> is at most
        /// <paramref name="_ThreatLevel"/> and that are still allowed to fire.
        /// </summary>
        /// <param name="_ThreatLevel">The current threat level.</param>
        /// <returns>Matching punishers in ascending threat-rating order.</returns>
        public List<IPunisher> GetPunisher(UInt32 _ThreatLevel)
        {
            List<IPunisher> var_Punisher = this.punishers.Where(p => p.IsActive && p.ThreatRating <= _ThreatLevel && (!p.PunishOnce || p.PunishOnce && !p.HasPunished)).ToList();

            return var_Punisher;
        }

        /// <summary>
        /// Removes a previously registered punisher.
        /// </summary>
        /// <param name="_Punisher">The punisher to remove.</param>
        private void RemovePunisher(IPunisher _Punisher)
        {
            if (this.punishers.Contains(_Punisher))
            {
                this.punishers.Remove(_Punisher);
            }
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Discovers and registers all supported detectors and punishers under the monitor's children, then sorts
        /// punishers by ascending threat rating.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Bail if the singleton base destroyed this duplicate.
            if(this == null || this.gameObject == null)
            {
                return;
            }

            foreach (var var_Detector in this.GetComponentsInChildren<IDetector>())
            {
                if (var_Detector.IsSupported)
                {
                    this.RegisterDetector(var_Detector);
                }
            }

            foreach (var var_Punisher in this.GetComponentsInChildren<IPunisher>())
            {
                if (var_Punisher.IsSupported)
                {
                    this.RegisterPunisher(var_Punisher);
                }
            }

            this.punishers.Sort((a, b) => a.ThreatRating.CompareTo(b.ThreatRating));
        }

        /// <summary>
        /// Decays the threat level by a factor that grows when <see cref="SensitiveLevel"/> is low.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            float var_Reduction = UnityEngine.Time.fixedUnscaledDeltaTime / 5.5f * ((int)ESensitiveLevel.VERY_SENSITIVE + 1 - (int)this.SensitiveLevel);

            this.threatLevel = Mathf.Max(0, this.threatLevel - var_Reduction);
        }

        #endregion
    }
}
