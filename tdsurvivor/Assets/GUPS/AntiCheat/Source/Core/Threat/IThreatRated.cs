// System
using System;

namespace GUPS.AntiCheat.Core.Threat
{
    /// <summary>
    /// Contract for types that expose a numeric threat rating, where higher values denote greater perceived threats.
    /// </summary>
    public interface IThreatRated
    {
        /// <summary>
        /// Gets the assessed threat level as a 32-bit unsigned integer.
        /// </summary>
        UInt32 ThreatRating { get; }
    }
}
