using System;
using _KITSystem.EventBus;
using Unity.Mathematics;

namespace _Games.Battle
{
    public struct QueryAgentSignal : ISignal
    {
        public readonly float Radius;
        public readonly float2 Position;
        public readonly Action<(int count, AgentData[] agentsData)> OnQueryAgent;

        public QueryAgentSignal(float2 position, float radius, Action<(int count, AgentData[] agentsData)> onQueryAgent)
        {
            Position = position;
            Radius = radius;
            OnQueryAgent = onQueryAgent;
        }
    }
}