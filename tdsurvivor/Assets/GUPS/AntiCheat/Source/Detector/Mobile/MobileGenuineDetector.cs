// System
using System;
using System.Collections;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Detector.Mobile
{
    /// <summary>
    /// Detects whether the built mobile app (Android, iOS) is genuine, using Unity's
    /// <see cref="Application.genuine"/> check.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public class MobileGenuineDetector : ADetector
    {
        // Name
        #region Name

        /// <inheritdoc/>
        public override String Name => "Mobile Genuine Check Detector";

        #endregion

        // Platform
        #region Platform

        /// <inheritdoc/>
#if UNITY_ANDROID || UNITY_IOS
        public override bool IsSupported => true;
#else
        public override bool IsSupported => false;
#endif

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Gets the false-positive likelihood reported with each detection.
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

        /// <inheritdoc/>
        public override void OnNext(IWatchedSubject _Subject)
        {
            // Does not observe any subjects.
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

        // Genuine
        #region Genuine

        /// <summary>
        /// Run the genuine check only once on detector start. The genuine check can be resource intensive.
        /// </summary>
        [Header("Genuine - Settings")]
        [Tooltip("Enable to check if the application is genuine only on detector start. The genuine check can be resource intensive. Disable to check in a define interval. Recommended: True")]
        public bool CheckGenuineOnlyOnGameStart = true;

        /// <summary>
        /// Interval in seconds between genuine checks.
        /// </summary>
        [Tooltip("Interval in seconds in which to check the genuine of the application. Recommended: 60")]
        [Range(0.001f, 600f)]
        public float RecheckIntervalForPossibleCheating = 60f;

        /// <summary>
        /// Coroutine that performs a periodic genuine check while the detector is active.
        /// </summary>
        /// <returns>A coroutine enumerator.</returns>
        private IEnumerator CheckGenuine()
        {
            while (true)
            {
                if (this.IsActive)
                {
                    if (!this.CheckGenuineOnlyOnGameStart)
                    {
                        if (!Application.genuine)
                        {
                            this.PossibleCheatingDetected = true;

                            this.Notify(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

                            this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));
                        }
                    }
                }

                yield return new WaitForSecondsRealtime(this.RecheckIntervalForPossibleCheating);
            }
        }

        /// <summary>
        /// Performs a single genuine check on demand.
        /// </summary>
        /// <returns><c>true</c> if the application is genuine (or the check is unavailable); <c>false</c> if tampering was detected.</returns>
        public bool ManualGenuineCheck()
        {
            if (Application.genuineCheckAvailable)
            {
                if (!Application.genuine)
                {
                    this.PossibleCheatingDetected = true;

                    this.Notify(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

                    this.OnCheatingDetectionEvent?.Invoke(new CheatingDetectionStatus(this.PossibilityOfFalsePositive, this.ThreatRating));

                    return false;
                }
            }

            return true;
        }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Runs the genuine check on start and starts the periodic recheck loop when supported by the platform.
        /// </summary>
        protected virtual void Start()
        {
            if (this.IsActive)
            {
                if (this.CheckGenuineOnlyOnGameStart)
                {
                    this.ManualGenuineCheck();
                }
            }

            // Start the periodic recheck only when the platform supports Application.genuine.
            if (Application.genuineCheckAvailable)
            {
                this.StartCoroutine(this.CheckGenuine());
            }
        }

        #endregion
    }
}
