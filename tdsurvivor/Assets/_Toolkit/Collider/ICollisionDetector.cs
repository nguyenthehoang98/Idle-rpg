using System;
using _Toolkit.Shared;
using _Toolkit.Updater;

namespace _Toolkit.Collider
{
    public interface ICollisionDetector : ITickRunner
    {
        event Action<Unique> OnOverlapped;

        void Startup();
        void Shutdown();
    }
}