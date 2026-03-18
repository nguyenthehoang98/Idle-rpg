using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct DefaultStatData : IComponentData
    {
        public readonly float MoveSpeed;
        public readonly int Attack;
        public readonly int Health;

        public DefaultStatData(float moveSpeed, int attack, int health)
        {
            MoveSpeed = moveSpeed;
            Attack = attack;
            Health = health;
        }
    }
}