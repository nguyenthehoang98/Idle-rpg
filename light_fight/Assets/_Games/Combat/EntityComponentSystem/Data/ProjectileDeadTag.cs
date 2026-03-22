using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileDeadTag : IComponentData
    {
        public ProjectileDeadReason Reason;
        public bool IsTrigger;

        public ProjectileDeadTag(ProjectileDeadReason reason)
        {
            Reason = reason;
            IsTrigger = false;
        }
    }
    
    public enum ProjectileDeadReason
    {
        EndCycle,
        Hit,
    }
}