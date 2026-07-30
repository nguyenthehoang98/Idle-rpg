// System
using System;

namespace GUPS.AntiCheat.Core.Watch
{
    /// <summary>
    /// An observer that receives notifications from one or more <see cref="IWatchedSubject"/>s.
    /// </summary>
    /// <typeparam name="TWatchedSubject">The <see cref="IWatchedSubject"/> type this observer accepts.</typeparam>
    public interface IWatcher<in TWatchedSubject> : IObserver<TWatchedSubject>, IDisposable
        where TWatchedSubject : IWatchedSubject
    {
        // This interface does not declare any additional members.
        // It inherits the members from IObserver<T> where T : IWatchedSubject.

        // Members from IObserver<T>:
        // - void OnCompleted()
        // - void OnError(Exception error)
        // - void OnNext(T value)

        // Members from IDisposable:
        // - void Dispose()
    }
}
