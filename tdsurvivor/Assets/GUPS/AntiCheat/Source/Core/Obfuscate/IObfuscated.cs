namespace GUPS.AntiCheat.Core.Obfuscate
{
    /// <summary>
    /// Contract for types that apply obfuscation to their own content.
    /// </summary>
    public interface IObfuscated
    {
        /// <summary>
        /// Applies obfuscation to this instance's content.
        /// </summary>
        void Obfuscate();
    }
}
