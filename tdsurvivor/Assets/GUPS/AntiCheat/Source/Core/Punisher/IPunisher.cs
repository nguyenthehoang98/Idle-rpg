// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Threat;

namespace GUPS.AntiCheat.Core.Punisher
{
    /// <summary>
    /// Administers punitive actions once the perceived threat reaches the configured <see cref="IThreatRated.ThreatRating"/>.
    /// </summary>
    public interface IPunisher : IThreatRated
    {
        // Members from IThreatRated:
        // - UInt32 ThreatRating { get; }

        /// <summary>
        /// Gets the human-readable name of the punisher.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the punisher is supported on the current platform.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the punisher is active and able to administer actions.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets a value indicating whether the punisher fires at most once (<c>true</c>) or every time the threat threshold is exceeded (<c>false</c>).
        /// </summary>
        bool PunishOnce { get; }

        /// <summary>
        /// Gets a value indicating whether the punisher has already administered an action.
        /// </summary>
        bool HasPunished { get; }

        /// <summary>
        /// Administers the punitive action.
        /// </summary>
        void Punish();
    }
}
