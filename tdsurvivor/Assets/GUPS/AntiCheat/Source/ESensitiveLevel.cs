namespace GUPS.AntiCheat
{
    /// <summary>
    /// Reaction sensitivity used by <see cref="AntiCheatMonitor"/> to scale incoming threat ratings.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.AntiCheatMonitor"/>
    public enum ESensitiveLevel : byte
    {
        /// <summary>
        /// Ignore all reported threats.
        /// </summary>
        NOT_SENSITIVE = 0,

        /// <summary>
        /// React only to high threat ratings.
        /// </summary>
        LESS_SENSITIVE = 1,

        /// <summary>
        /// React to moderate threat ratings.
        /// </summary>
        MODERATE = 2,

        /// <summary>
        /// React to low threat ratings. May produce false positives.
        /// </summary>
        SENSITIVE = 3,

        /// <summary>
        /// React to any threat rating. May produce false positives.
        /// </summary>
        VERY_SENSITIVE = 4,
    }
}
