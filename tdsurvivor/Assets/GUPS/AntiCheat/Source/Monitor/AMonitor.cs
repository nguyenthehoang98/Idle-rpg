// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Monitor;
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Monitor
{
    /// <summary>
    /// Abstract base class for monitors in a Unity environment.
    /// </summary>
    /// <remarks>
    /// Provides the lifecycle (start, pause, resume, stop) and the observer pattern via
    /// <see cref="IWatchAble{IWatchedSubject}"/> for all concrete monitors.
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public abstract class AMonitor : MonoBehaviour, IMonitor, IWatchAble<IWatchedSubject>
    {
        // Name
        #region Name

        /// <summary>
        /// Gets the display name of the monitor.
        /// </summary>
        public abstract String Name { get; }

        #endregion

        // Lifecycle
        #region Lifecycle

        /// <summary>
        /// Whether the monitor is active and should be running (default: true).
        /// </summary>
        [SerializeField]
        [Header("General - Settings")]
        [Tooltip("A value indicating whether the monitor is active and should be running (Default: true).")]
        private bool isActive = true;

        /// <summary>
        /// Gets a value indicating whether the monitor is active and should be running.
        /// </summary>
        public bool IsActive => this.isActive;

        /// <summary>
        /// Gets a value indicating whether the monitor has been started and is currently running.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the monitor is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Starts the monitor, beginning data collection and processing.
        /// </summary>
        public void Start()
        {
            // Guard against re-entrant or duplicate starts.
            if (this.IsStarted)
            {
                return;
            }

            // Flip the started flag before delegating so observers can rely on IsStarted inside OnStart.
            this.IsStarted = true;

            // Hand off to the concrete monitor to perform any custom startup work.
            this.OnStart();
        }

        /// <summary>
        /// Called when the monitor is started. Override to provide custom start behavior.
        /// </summary>
        protected virtual void OnStart()
        {

        }

        /// <summary>
        /// Pauses the monitor, temporarily suspending data collection without terminating it.
        /// </summary>
        public void Pause()
        {
            // Skip if already paused or never started; pausing a stopped monitor is a no-op.
            if (this.IsPaused)
            {
                return;
            }

            if (!this.IsStarted)
            {
                return;
            }

            // Mark as paused first so OnPause hooks observe a consistent state.
            this.IsPaused = true;

            this.OnPause();
        }

        /// <summary>
        /// Called when the monitor is paused. Override to provide custom pause behavior.
        /// </summary>
        protected virtual void OnPause()
        {

        }

        /// <summary>
        /// Resumes the monitor after a pause.
        /// </summary>
        public void Resume()
        {
            // Only a paused, started monitor can be resumed.
            if (!this.IsPaused)
            {
                return;
            }

            if (!this.IsStarted)
            {
                return;
            }

            // Clear the paused flag before invoking the resume hook.
            this.IsPaused = false;

            this.OnResume();
        }

        /// <summary>
        /// Called when the monitor is resumed. Override to provide custom resume behavior.
        /// </summary>
        protected virtual void OnResume()
        {

        }

        /// <summary>
        /// Stops the monitor and finalizes any cleanup.
        /// </summary>
        public void Stop()
        {
            // Stopping a non-started monitor is a no-op.
            if (!this.IsStarted)
            {
                return;
            }

            // Clear the started flag before the stop hook so derived classes see a fully stopped state.
            this.IsStarted = false;

            this.OnStop();
        }

        /// <summary>
        /// Called when the monitor is stopped. Override to provide custom stop behavior.
        /// </summary>
        protected virtual void OnStop()
        {

        }

        /// <summary>
        /// Unity update callback; invokes <see cref="OnUpdate"/> while the monitor is started, not paused and active.
        /// </summary>
        private void Update()
        {
            // Skip the per-frame hook unless the monitor is started, not paused, and currently active.
            if (!this.IsStarted || this.IsPaused)
            {
                return;
            }

            if (!this.isActive)
            {
                return;
            }

            this.OnUpdate();
        }

        /// <summary>
        /// Called on each Unity update while the monitor is started, not paused and active. Override to implement custom update behavior.
        /// </summary>
        protected virtual void OnUpdate()
        {

        }

        /// <summary>
        /// Stops the monitor when the underlying <see cref="MonoBehaviour"/> is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Ensure a clean shutdown if the GameObject is destroyed while the monitor is still running.
            this.Stop();
        }

        /// <summary>
        /// Notifies all subscribed observers that the monitor has completed and clears the observer list.
        /// </summary>
        public void Dispose()
        {
            // Broadcast completion to every still-living observer so they can unsubscribe / release resources.
            foreach (var var_Observer in this.observers)
            {
                if (var_Observer == null)
                {
                    continue;
                }

                var_Observer.OnCompleted();
            }

            // Drop the observer list so no further callbacks fire after disposal.
            this.observers.Clear();
        }

        #endregion

        // Observable
        #region Observable

        /// <summary>
        /// The observers subscribed to the monitor.
        /// </summary>
        private List<IObserver<IWatchedSubject>> observers = new List<IObserver<IWatchedSubject>>();

        /// <summary>
        /// Subscribes an observer to receive notifications from the monitor.
        /// </summary>
        /// <param name="_Observer">The observer to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that unsubscribes the observer when disposed.</returns>
        public IDisposable Subscribe(IObserver<IWatchedSubject> _Observer)
        {
            // De-duplicate; subscribing the same observer twice would cause double notifications.
            if (!this.observers.Contains(_Observer))
            {
                this.observers.Add(_Observer);
            }

            // Return a handle so the caller can detach without exposing the observer list.
            return new Unsubscriber(this.observers, _Observer);
        }

        /// <summary>
        /// Notifies all subscribed observers with the provided watched subject.
        /// </summary>
        /// <param name="_Subject">The watched subject to forward to observers.</param>
        public void Notify(IWatchedSubject _Subject)
        {
            // Fan the subject out to every subscribed observer; nulls are tolerated to survive collected MonoBehaviours.
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
        /// Disposable that unsubscribes a single observer from the monitor.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<IWatchedSubject>> observers;
            private IObserver<IWatchedSubject> observer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Unsubscriber"/> class.
            /// </summary>
            /// <param name="observers">The observer list to remove from on dispose.</param>
            /// <param name="observer">The observer to unsubscribe.</param>
            public Unsubscriber(List<IObserver<IWatchedSubject>> observers, IObserver<IWatchedSubject> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            /// <summary>
            /// Removes the observer from the observer list.
            /// </summary>
            public void Dispose()
            {
                // Remove only if the observer is still registered; safe to call multiple times.
                if (this.observer != null && this.observers.Contains(this.observer))
                {
                    this.observers.Remove(this.observer);
                }
            }
        }

        #endregion
    }
}
