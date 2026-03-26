using ProjectDawn.Custom;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentForceSystemGroup))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    public partial class AgentLocomotionSystemGroup : ComponentSystemGroup { }
}
