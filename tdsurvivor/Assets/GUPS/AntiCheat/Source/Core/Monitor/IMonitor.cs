namespace GUPS.AntiCheat.Core.Monitor
{
    /// <summary>
    /// Lifecycle contract for monitors that collect and process data about the host OS, game, or application.
    /// </summary>
    public interface IMonitor
    {
        /// <summary>
        /// Gets the human-readable name of the monitor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the monitor has been started and is currently running.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Gets a value indicating whether the monitor is currently paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Starts the monitor and begins collecting data.
        /// </summary>
        void Start();

        /// <summary>
        /// Temporarily suspends data collection without terminating the monitor.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes a previously paused monitor.
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the monitor and performs any required cleanup.
        /// </summary>
        void Stop();
    }
}
