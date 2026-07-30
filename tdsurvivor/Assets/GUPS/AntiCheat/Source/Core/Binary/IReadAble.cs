namespace GUPS.AntiCheat.Core.Binary
{
    /// <summary>
    /// Contract for types that can deserialize themselves from a <see cref="BinaryReader"/>.
    /// </summary>
    internal interface IReadAble
    {
        /// <summary>
        /// Reads this instance's state from the supplied binary reader.
        /// </summary>
        /// <param name="_Reader">The binary reader positioned at the start of the encoded data.</param>
        void Read(BinaryReader _Reader);
    }
}
