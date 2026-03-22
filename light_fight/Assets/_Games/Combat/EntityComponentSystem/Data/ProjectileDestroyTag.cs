using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileDestroyTag : IComponentData
    {
        public ProjectileDeadReason Reason;
        public bool IsTrigger;

        public ProjectileDestroyTag(ProjectileDeadReason reason)
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