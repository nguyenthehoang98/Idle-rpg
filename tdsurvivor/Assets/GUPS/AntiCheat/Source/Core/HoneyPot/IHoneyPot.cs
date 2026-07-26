// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

namespace GUPS.AntiCheat.Core.HoneyPot
{
    /// <summary>
    /// A deceptive element planted to attract and reveal unauthorized or malicious actions; observers are notified when it is tripped.
    /// </summary>
    public interface IHoneyPot : IWatchAble<IWatchedSubject>
    {        
        // Members from IWatchAble<IWatchedSubject> interface:
        // - IDisposable Subscribe(IObserver<T> observer)
        // - void Dispose()

        /// <summary>
        /// Places the honey pot so it becomes active and can be tripped.
        /// </summary>
        void PlaceHoneyPot();
    }
}
