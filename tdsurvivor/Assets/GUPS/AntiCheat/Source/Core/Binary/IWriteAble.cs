namespace GUPS.AntiCheat.Core.Binary
{
    /// <summary>
    /// Contract for types that can serialize themselves to a <see cref="BinaryWriter"/>.
    /// </summary>
    internal interface IWriteAble
    {
        /// <summary>
        /// Writes this instance's state to the supplied binary writer.
        /// </summary>
        /// <param name="_Writer">The binary writer to append to.</param>
        void Write(BinaryWriter _Writer);
    }
}
