using System;
using _GameToolkit.Updater;

namespace _GameToolkit.Collider
{
    public sealed class ColliderTickRunner : TickRunner<CollisionDetector>, IDisposable
    {
        public static ColliderTickRunner Instance { get; private set; }

        public ColliderTickRunner()
        {
            if(Instance == null) Instance = this; 
        }

        public void Dispose()
        {
            if (Instance != null && Instance == this) Instance = null;
        }
    }
}