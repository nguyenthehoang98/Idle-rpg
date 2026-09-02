using System;
using _GameToolkit.Shared;
using _Toolkit.Updater;

namespace _GameToolkit.Collider
{
    public interface ICollisionDetector : ITickRunner
    {
        event Action<Unique> OnOverlapped;

        void Startup();
        void Shutdown();
    }
}