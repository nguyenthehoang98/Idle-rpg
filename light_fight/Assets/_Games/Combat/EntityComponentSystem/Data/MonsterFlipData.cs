using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct MonsterFlipData : IComponentData
    {
        public bool FacingRight;
        public bool Changed;
    }
}