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
            SystemBus.Subscribe<WeaponQueryAgentSignal>(OnWeaponQueryAgent);
            SystemBus.Subscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Subscribe<CircleShapeHitSignal>(OnCircleShapeHit);
        }

        public override void Dispose()
        {
            SystemBus.Unsubscribe<WeaponQueryAgentSignal>(OnWeaponQueryAgent);
            SystemBus.Unsubscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Unsubscribe<CircleShapeHitSignal>(OnCircleShapeHit);
            base.Dispose();
        }

        private void OnWeaponQueryAgent(WeaponQueryAgentSignal signal)
        {
            float signalRadius = signal.Radius;
            float2 signalPos = signal.Position;
            float2 signalSize = new float2(signalRadius * 2, signalRadius * 2);
            int count = QueryAgent(signalPos, signalSize, out AgentData[] agents);
            signal.OnQueryAgent?.Invoke((count, agents));
        }

        private void OnSquareShapeHit(SquareShapeHitSignal signal)
        {
            float2 signalPos = signal.Position;
            float2 signalSize = signal.Size;
            int count = QueryAgent(signalPos, signalSize, out AgentData[] agents);

            float2 half = signalSize * 0.5f;
            float left = signalPos.x - half.x;
            float right = signalPos.x + half.x;
            float top = signalPos.y + half.y;
            float bottom = signalPos.y - half.y;
            
            List<int> results = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];

                float2 p = agent.position;
                float r = agent.radius;
                
                float closestX = math.clamp(p.x, left, right);
                float closestY = math.clamp(p.y, bottom, top);

                float dx = p.x - closestX;
                float dy = p.y - closestY;
                
                if (dx * dx + dy * dy <= r * r)
                {
                    results.Add(agent.agent);
                }
            }
            
            signal.Agents.Invoke(results);
        }

        private void OnCircleShapeHit(CircleShapeHitSignal signal)
        {
            float signalRadius = signal.Radius;
            float2 signalPos = signal.Position;
            float2 signalSize = new float2(signalRadius * 2, signalRadius * 2);
            
            int count = QueryAgent(signalPos, signalSize, out AgentData[] agents);
            
            List<int> results = new List<int>(count);
          
            float radiusSq;

            for (int i = 0; i < count; i++)
            {
                AgentData agent = agents[i];

                float totalRadius = signalRadius + agent.radius;

                float2 delta = agent.position - signal.Position;

                radiusSq = totalRadius * totalRadius;

                if (math.lengthsq(delta) <= radiusSq)
                {
                    results.Add(agent.agent);
                }
            }
            
            signal.Agents.Invoke(results);
        }
    }
}