namespace _Game.AbilitySystem
{
    [System.Flags]
    public enum StatusEffect
    {
        None = 0,
        Stun = 1 << 0,
        KnockBack = 1 << 1,
        Slow = 1 << 2,
    }

    public static class StatusEffectMethod
    {
        public static bool Has(this StatusEffect current, StatusEffect flag)
        {
            return (current & flag) == flag;
        }
        
        public static void Add(ref StatusEffect current, StatusEffect flag)
        {
            current |= flag;
        }

        public static void Remove(ref StatusEffect current, StatusEffect flag)
        {
            current &= ~flag;
        }
    }
}