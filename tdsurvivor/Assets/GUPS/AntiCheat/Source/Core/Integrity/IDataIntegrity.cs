namespace GUPS.AntiCheat.Core.Integrity
{
    /// <summary>
    /// Contract for types that maintain and verify the integrity of their own data.
    /// </summary>
    public interface IDataIntegrity
    {
        /// <summary>
        /// Gets a value indicating whether the last integrity check succeeded.
        /// </summary>
        bool HasIntegrity { get; }

        /// <summary>
        /// Re-checks the integrity of the contained data.
        /// </summary>
        /// <returns><c>true</c> when the data is intact; otherwise <c>false</c>.</returns>
        bool CheckIntegrity();
    }
}