// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Abstract base <see cref="MonoBehaviour"/> for detectors that watch for possible cheating and publish detections
    /// to their observers.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IDetector"/> and provides a default observer list, subscribe / notify plumbing and a
    /// platform support check in <see cref="Awake"/>.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    /// <seealso cref="PrimitiveCheatingDetector"/>
    public abstract class ADetector : MonoBehaviour, IDetector
    {
        // Name
        #region Name

        /// <summary>
        /// Gets the human-readable name of the detector.
        /// </summary>
        public abstract String Name { get; }

        #endregion

        // Platform
        #region Platform

        /// <summary>
        /// Gets a value indicating whether the detector is supported on the current platform.
        /// </summary>
        public abstract bool IsSupported { get; }

        /// <summary>
        /// Backing field for <see cref="IsActive"/> (Default: true).
        /// </summary>
        [SerializeField]
        [Header("General - Settings")]
        [Tooltip("Gets or sets whether the detector is active and watching for possible cheating (Default: true).")]
        private bool isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether the detector is active and watching for possible cheating.
        /// </summary>
        public bool IsActive { get => this.isActive; set => this.isActive = value; }

        #endregion

        // Threat Rating
        #region Threat Rating

        /// <summary>
        /// Gets the threat rating reported with every detection. Higher values denote greater perceived threats.
        /// </summary>
        public abstract uint ThreatRating { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the detector has observed possible cheating activity.
        /// </summary>
        public abstract bool PossibleCheatingDetected { get; protected set; }

        #endregion

        // Observable
        #region Observable

        /// <summary>
        /// The list of observers currently subscribed to this detector.
        /// </summary>
        private List<IObserver<IDetectorStatus>> observers = new List<IObserver<IDetectorStatus>>();

        /// <summary>
        /// Subscribes an observer to receive future detection notifications.
        /// </summary>
        /// <param name="_Observer">The observer to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that unsubscribes the observer when disposed.</returns>
        public IDisposable Subscribe(IObserver<IDetectorStatus> _Observer)
        {
            // Append the observer to the list only when it is not already subscribed - guards against duplicate
            // notifications when callers subscribe twice.
            if (!this.observers.Contains(_Observer))
            {
                this.observers.Add(_Observer);
            }

            // Hand back a disposable token so the caller can unsubscribe by disposing it (RAII style).
            return new Unsubscriber(this.observers, _Observer);
        }

        /// <summary>
        /// Notifies every subscribed observer of the given detection status.
        /// </summary>
        /// <param name="_Subject">The detection status to broadcast.</param>
        public void Notify(IDetectorStatus _Subject)
        {
            // Broadcast the detection to every still-alive observer; null slots are tolerated so a destroyed
            // MonoBehaviour observer does not break the broadcast.
            foreach (var var_Observer in this.observers)
            {
                if (var_Observer == null)
                {
                    continue;
                }

                var_Observer.OnNext(_Subject);
            }
        }

        /// <summary>
        /// Disposable token returned by <see cref="Subscribe"/> that removes the observer from the detector when disposed.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            /// <summary>
            /// The shared observer list owned by the detector.
            /// </summary>
            private List<IObserver<IDetectorStatus>> observers;

            /// <summary>
            /// The observer to remove on dispose.
            /// </summary>
            private IObserver<IDetectorStatus> observer;

            /// <summary>
            /// Creates a new unsubscriber for the given observer list and observer.
            /// </summary>
            /// <param name="observers">The observer list owned by the detector.</param>
            /// <param name="observer">The observer to unsubscribe on dispose.</param>
            public Unsubscriber(List<IObserver<IDetectorStatus>> observers, IObserver<IDetectorStatus> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            /// <summary>
            /// Removes the observer from the detector if it is still subscribed.
            /// </summary>
            public void Dispose()
            {
                // Remove the captured observer from the detector's list - safe to call repeatedly thanks to the
                // Contains guard.
                if (this.observer != null && this.observers.Contains(this.observer))
                {
                    this.observers.Remove(this.observer);
                }
            }
        }

        /// <summary>
        /// Notifies every subscribed observer that the detector has completed and clears the observer list.
        /// </summary>
        public void Dispose()
        {
            // Signal completion to every still-alive observer so they can release resources or unsubscribe.
            foreach (var var_Observer in this.observers)
            {
                if (var_Observer == null)
                {
                    continue;
                }

                var_Observer.OnCompleted();
            }

            // Drop all observer references so the detector and its observers can be garbage collected.
            this.observers.Clear();
        }

        #endregion

        // Observer
        #region Observer

        /// <summary>
        /// Called when an observed subject signals completion.
        /// </summary>
        public abstract void OnCompleted();

        /// <summary>
        /// Called when an observed subject reports an error.
        /// </summary>
        /// <param name="_Error">The error reported by the observed subject.</param>
        public abstract void OnError(Exception _Error);

        /// <summary>
        /// Called when an observed subject publishes a new status.
        /// </summary>
        /// <param name="_Subject">The status published by the observed subject.</param>
        public abstract void OnNext(IWatchedSubject _Subject);

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Disables the detector when <see cref="IsSupported"/> is <c>false</c> on the current platform.
        /// </summary>
        protected virtual void Awake()
        {
            // Self-disable on platforms the detector does not support so its Update / coroutines never run.
            if (!this.IsSupported)
            {
                this.isActive = false;
                this.enabled = false;
            }
        }

        #endregion
    }
}
