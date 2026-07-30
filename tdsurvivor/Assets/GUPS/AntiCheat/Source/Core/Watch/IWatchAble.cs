// System
using System;

namespace GUPS.AntiCheat.Core.Watch
{
    /// <summary>
    /// An observable subject that other components can subscribe to via the observer pattern.
    /// </summary>
    /// <typeparam name="TWatchedSubject">The <see cref="IWatchedSubject"/> type that is pushed to subscribers.</typeparam>
    public interface IWatchAble<out TWatchedSubject> : IObservable<TWatchedSubject>, IDisposable
        where TWatchedSubject : IWatchedSubject
    {
        // This interface does not declare any additional members.

        // Members from IObservable<T>:
        // - IDisposable Subscribe(IObserver<T> observer)

        // Members from IDisposable:
        // - void Dispose()
    }

}
