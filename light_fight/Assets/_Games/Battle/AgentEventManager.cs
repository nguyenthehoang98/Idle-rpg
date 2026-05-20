using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime.Signal;
using Unity.Mathematics;

namespace _Games.Battle
{
    // [Don't remove]
    public sealed class AgentEventManager : AgentGrid, ITickable
    {
        protected override void OnInitialize()
        {
            SystemBus.Subscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Subscribe<CircleShapeHitSignal>(OnCircleShapeHit);
        }

        public override void Dispose()
        {
            SystemBus.Unsubscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Unsubscribe<CircleShapeHitSignal>(OnCircleShapeHit);
            base.Dispose();
        }

        private void OnSquareShapeHit(SquareShapeHitSignal signal)
        {
            int count = QueryAgent(signal.Position, signal.Size, out AgentData[] agents);
            List<int> results = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];
                float2 agentPos = agent.position;
                results[i] = agents[i].agent;
            }
            signal.Agents.Invoke(results);
        }

        private void OnCircleShapeHit(CircleShapeHitSignal signal)
        {
            int count = QueryAgent(signal.Position, new float2(signal.Radius, signal.Radius), out AgentData[] agents);
            List<int> results = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                results[i] = agents[i].agent;
            }
            signal.Agents.Invoke(results);
        }
    }
}