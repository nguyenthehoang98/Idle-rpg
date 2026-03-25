using _Games.Combat.EntityComponentSystem.Model;
using Unity.Burst;
using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.System
{
    [BurstCompile]
    [UpdateAfter(typeof(ProjectileOverlapSystem))]
    [UpdateInGroup(typeof(BattleSimulationGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial struct ProjectileDamageMonsterSystem : ISystem
    {
    }
}