using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime.Signal
{
    public struct SquareShapeHitSignal : EventBus.ISignal
    {
        public float2 Position;
        public float2 Size;
        public Action<List<int>> Entities;
        
        public SquareShapeHitSignal(float2 position, float2 size, Action<List<int>> entities)
        {
            Position = position;
            Size = size;
            Entities = entities;
        }
    }
}