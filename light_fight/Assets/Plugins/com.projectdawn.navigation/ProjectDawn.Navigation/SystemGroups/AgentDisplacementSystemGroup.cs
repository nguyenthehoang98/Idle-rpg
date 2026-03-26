using ProjectDawn.Custom;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentLocomotionSystemGroup))]
    [UpdateInGroup(typeof(CustomSimulationGroup))]
    public partial class AgentDisplacementSystemGroup : ComponentSystemGroup { }
}
