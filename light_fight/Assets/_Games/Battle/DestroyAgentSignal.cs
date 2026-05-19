using _KITSystem.EventBus;

namespace _Games.Battle
{
    public struct DestroyAgentSignal : ISignal
    {
        public readonly int Agent;

        public DestroyAgentSignal(int agent)
        {
            Agent = agent;
        }
    }
}