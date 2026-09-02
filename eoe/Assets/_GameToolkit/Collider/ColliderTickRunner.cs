using System;
using _Toolkit.Updater;

namespace _GameToolkit.Collider
{
    public sealed class ColliderTickRunner : BaseTickRunner<ICollisionDetector>, IDisposable
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