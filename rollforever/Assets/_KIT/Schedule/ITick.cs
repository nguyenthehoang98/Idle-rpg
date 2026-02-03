using System;

namespace _KIT.Schedule
{
    public interface ITick : IDisposable
    {
        void OnUpdate(float deltaTime);
        
        int Order { get; }
    }
}