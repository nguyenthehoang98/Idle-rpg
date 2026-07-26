using System;
using _GameToolkit.Shared;

namespace _GameToolkit.Collision
{
    public interface ICollision
    {
        event Action<EntityId> OnOverlap;
        
        void Startup();
        void Tick();
        void Shutdown();
    }
}