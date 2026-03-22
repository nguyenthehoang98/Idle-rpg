using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileOverlapSystem))]
    public partial struct ProjectileDamageMonsterSystem : ISystem
    {
    }
}