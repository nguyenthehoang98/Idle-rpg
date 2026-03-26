using ProjectDawn.Custom;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentActionSystemGroup))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    public partial class AgentSeekingSystemGroup : ComponentSystemGroup { }
}
