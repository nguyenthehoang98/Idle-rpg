// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Integrity;
using GUPS.AntiCheat.Core.Obfuscate;
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Core.Protected
{
    /// <summary>
    /// A protected value that obfuscates itself, verifies its own integrity, and can be watched as a subject.
    /// </summary>
    public interface IProtected : IObfuscated, IDataIntegrity, IWatchedSubject
    {
        // Members from IObfuscated interface:
        // - void Obfuscate()

        // Members from IDataIntegrity interface:
        // - bool HasIntegrity { get; }
        // - bool CheckIntegrity()

        /// <summary>
        /// Gets the protected value (deobfuscated on access).
        /// </summary>
        public object Value { get;  }
    }
}
