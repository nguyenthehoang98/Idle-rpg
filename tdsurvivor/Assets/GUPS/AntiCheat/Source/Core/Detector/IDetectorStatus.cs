// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Threat;
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Core.Detector
{
    /// <summary>
    /// Status payload published by an <see cref="IDetector"/>, combining a threat rating with a false-positive likelihood.
    /// </summary>
    public interface IDetectorStatus : IWatchedSubject, IThreatRated
    {
        /// <summary>
        /// Gets the probability that the reported threat is a false positive, in the range <c>[0.0, 1.0]</c>.
        /// </summary>
        float PossibilityOfFalsePositive { get; }
    }
}
