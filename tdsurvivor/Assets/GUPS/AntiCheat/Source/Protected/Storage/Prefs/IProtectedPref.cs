// System
using System;

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Property-style accessor for a single protected player preference of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value stored in the protected player preferences.</typeparam>
    /// <remarks>
    /// Implementations are typically <c>[Serializable]</c> wrappers around the static
    /// <see cref="ProtectedPlayerPrefs"/> facade, so a single preference can be assigned in the Unity inspector.
    /// </remarks>
    /// <seealso cref="ProtectedPlayerPrefs"/>
    /// <seealso cref="ProtectedIntPref"/>
    public interface IProtectedPref<T>
    {
        /// <summary>
        /// Gets the unique key used to identify the preference in storage.
        /// </summary>
        String Key { get; }

        /// <summary>
        /// Gets and sets the current value of the preference.
        /// </summary>
        T Value { get; set; }
    }
}
