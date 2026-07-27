using System;
using UnityEngine;

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