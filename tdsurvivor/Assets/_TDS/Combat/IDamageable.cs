namespace _TDS.Combat
{
    /// <summary>
    /// Entity nhận damage. Implement bởi Monster, Hero, BaseCore.
    /// EntityId chính là agent ID từ AgentSimulator (xem EntityQuery).
    /// </summary>
    public interface IDamageable
    {
        int EntityId { get; }

        void TakeDamage(int damage);

        bool IsAlive { get; }
    }
}
