using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime.Signal
{
    public struct CircleShapeHitEntitySignal : EventBus.ISignal
    {
        public float2 Position;
        public float Radius;
        public Action<List<int>> Entities;
        
        public CircleShapeHitEntitySignal(float2 position, float radius, Action<List<int>> entities)
        {
            Position = position;
            Radius = radius;
            Entities = entities;
        }
    }
}