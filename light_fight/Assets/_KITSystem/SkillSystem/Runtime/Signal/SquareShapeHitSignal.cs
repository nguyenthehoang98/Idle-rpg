using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime.Signal
{
    public struct SquareShapeHitSignal : EventBus.ISignal
    {
        public float2 Position;
        public float2 Size;
        public Action<List<int>> Agents;
        
        public SquareShapeHitSignal(float2 position, float2 size, Action<List<int>> agents)
        {
            Position = position;
            Size = size;
            Agents = agents;
        }
    }
}