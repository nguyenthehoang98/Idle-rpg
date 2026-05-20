using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime.Signal
{
    public struct CircleShapeHitSignal : _KITSystem.EventBus.ISignal
    {
        public float2 Position;
        public float Radius;
        public Action<List<int>> Agents;
        
        public CircleShapeHitSignal(float2 position, float radius, Action<List<int>> agents)
        {
            Position = position;
            Radius = radius;
            Agents = agents;
        }
    }
}