using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileDamageMonsterSystem))]
    public partial struct ProjectileCleanupSystem : ISystem
    {
    }
}