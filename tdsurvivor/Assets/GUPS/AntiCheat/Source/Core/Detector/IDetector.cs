// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;
using GUPS.AntiCheat.Core.Threat;

namespace GUPS.AntiCheat.Core.Detector
{
    /// <summary>
    /// Watches one or more <see cref="IWatchedSubject"/>s for possible cheating and publishes its status to observers.
    /// </summary>
    /// <remarks>
    /// Combines three roles: observer of watched subjects (<see cref="IWatcher{T}"/>), publisher of its own
    /// <see cref="IDetectorStatus"/> (<see cref="IWatchAble{T}"/>), and threat-rated source (<see cref="IThreatRated"/>).
    /// </remarks>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public interface IDetector : IWatcher<IWatchedSubject>, IWatchAble<IDetectorStatus>, IThreatRated
    {
        /// <summary>
        /// Gets the human-readable name of the detector.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the detector is supported on the current platform.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the detector is active and currently watching for cheating.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets a value indicating whether the detector has observed possible cheating activity.
        /// </summary>
        bool PossibleCheatingDetected { get; }
    }
}
