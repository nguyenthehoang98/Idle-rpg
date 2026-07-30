// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Default <see cref="IDetectorStatus"/> implementation carrying a false-positive likelihood and a threat rating.
    /// </summary>
    public struct CheatingDetectionStatus : IDetectorStatus
    {
        /// <summary>
        /// Gets the probability that the reported threat is a false positive, in the range <c>[0.0, 1.0]</c>.
        /// </summary>
        public float PossibilityOfFalsePositive { get; private set; }

        /// <summary>
        /// Gets the threat rating reported with this detection. Higher values denote greater perceived threats.
        /// </summary>
        public uint ThreatRating { get; private set; }

        /// <summary>
        /// Creates a new <see cref="CheatingDetectionStatus"/> with the given false-positive likelihood and threat rating.
        /// </summary>
        /// <param name="_PossibilityOfFalsePositive">The false-positive likelihood in the range <c>[0.0, 1.0]</c>.</param>
        /// <param name="_ThreatRating">The threat rating reported with the detection.</param>
        public CheatingDetectionStatus(float _PossibilityOfFalsePositive, uint _ThreatRating)
        {
            this.PossibilityOfFalsePositive = _PossibilityOfFalsePositive;
            this.ThreatRating = _ThreatRating;
        }
    }
}
