using _KITSystem.Data;

namespace _KITSystem.SkillSystem.Entity
{
    public struct EntityManagerBehaviourParameter
    {
        public EntityManagerBehaviourType type;
        public int entity;
        public ParameterValue[] values;
    }
    
    public enum EntityManagerBehaviourType
    {
        Created, Removed,
        BeHit
    }
}