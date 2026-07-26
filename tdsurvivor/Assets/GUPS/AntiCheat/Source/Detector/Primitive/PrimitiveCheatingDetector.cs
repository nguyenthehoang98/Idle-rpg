// System
using System;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Detects unexpected value modifications of protected primitive types, commonly caused by memory editing tools.
    /// </summary>
    /// <remarks>
    /// The detector is notified directly by the protected primitives (<see cref="GUPS.AntiCheat.Protected.IProtected"/>
    /// implementations) when they observe tampering. It does not poll any subjects on its own.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    /// <example>
    /// React to tampering detected by any <see cref="GUPS.AntiCheat.Protected.IProtected"/> primitive
    /// (e.g. <see cref="GUPS.AntiCheat.Protected.ProtectedInt32"/>) via the inspector-friendly Unity event:
    /// <code>
    /// using GUPS.AntiCheat;
    /// using GUPS.AntiCheat.Detector;
    ///
    /// var detector = AntiCheatMonitor.Instance.GetDetector&lt;PrimitiveCheatingDetector&gt;();
    /// detector.OnCheatingDetectionEvent.AddListener(status =&gt;
    /// {
    ///     Debug.LogWarning($"Cheat detected (threat={status.ThreatRating}, fp={status.PossibilityOfFalsePositive}).");
    /// });
    /// </code>
    /// </example>
    public class PrimitiveCheatingDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Primitive Cheating Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
        public override bool IsSupported => true;

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Gets the possibility of a false positive (in the range <c>[0.0, 1.0]</c>) reported with each detection.
        /// </summary>
        public float PossibilityOfFalsePositive => 0.01f;

        /// <summary>
        /// The threat rating reported on every detection (Recommended: 500).
        /// </summary>
        [SerializeField]
        [Header("Threat Rating - Settings")]
        [Tooltip("The threat rating of this detector. It is set to a very high value, because false positives are very unlikely and the impact of cheating is very high (Recommended: 500).")]
        private uint threatRating = 500;

        /// <inheritdoc/>
        public override uint ThreatRating { get => this.threatRating; protected set => this.threatRating = value; }

        /// <inheritdoc/>
        public override bool PossibleCheatingDetected { get; protected set; } = false;

        #endregion

        // Observable
        #region Observable

        /// <summary>
        /// Unity event raised on every detection. Useful to wire up reactions through the inspector without writing an
        /// <see cref="IObserver{T}"/>.
        /// </summary>
        [Header("Observable - Settings")]
        [Tooltip("A unity event that is used to subscribe to the cheating detection events. It is useful if you do not want to write custom observers to subscribe to the detectors and simply attach a callback to the detector event through the inspector.")]
        public CheatingDetectionEvent<CheatingDetectionStatus> OnCheatingDetectionEvent = new CheatingDetectionEvent<CheatingDetectionStatus>();

        #endregion

        // Observer
        #region Observer

        /// <summary>
        /// Invoked by the protected primitive types when they observe an unexpected modification. Notifies all
        /// subscribed observers and listeners of the detected cheating.
        /// </summary>
        /// <param name="_Subject">The watched subject reporting the modification.</param>
        public override void OnNext(IWatchedSubject _Subject)
        {
            if (this.IsActive)
            {
                this.PossibleCheatingDetected = true;

                this.Notify(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

                this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));
            }
        }

        /// <inheritdoc/>
        public override void OnError(Exception _Error)
        {
            // Does nothing.
        }

        /// <inheritdoc/>
        public override void OnCompleted()
        {
            // Does nothing.
        }

        #endregion
    }
}
