using ProjectDawn.Custom;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentPathingSystemGroup))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    public partial class AgentForceSystemGroup : ComponentSystemGroup { }
}
