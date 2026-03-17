using System;
using Unity.Entities;

namespace _Games.Combat.Model
{
    public class ShareData : IDisposable
    {
        public ShareData(Entity player)
        {
            Player = player;
        }

        public Entity Player { get; }
        
        public void Dispose()
        {
            
        }
    }
}