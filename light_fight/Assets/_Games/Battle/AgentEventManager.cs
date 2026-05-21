using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using Unity.Mathematics;

namespace _Games.Battle
{
    // [Don't remove]
    public sealed class AgentEventManager : AgentGrid, ITickable
    {
        protected override void OnInitialize()
        {
            SystemBus.Subscribe<WeaponQueryAgentSignal>(OnWeaponQueryAgent);
        }

        public override void Dispose()
        {
            SystemBus.Unsubscribe<WeaponQueryAgentSignal>(OnWeaponQueryAgent);
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
    }
}