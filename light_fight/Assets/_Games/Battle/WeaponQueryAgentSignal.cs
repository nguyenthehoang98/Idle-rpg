using System;
using _KITSystem.EventBus;
using _KITSystem.Grid;
using Unity.Mathematics;

namespace _Games.Battle
{
    public struct WeaponQueryAgentSignal : ISignal
    {
        public readonly float Radius;
        public readonly float2 Position;
        public readonly Action<(int count, AgentData[] agentsData)> OnQueryAgent;

        public WeaponQueryAgentSignal(float2 position, float radius, Action<(int count, AgentData[] agentsData)> onQueryAgent)
        {
            Position = position;
            Radius = radius;
            OnQueryAgent = onQueryAgent;
        }
    }
}