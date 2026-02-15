namespace _Game.Battle
{
    public readonly struct StatModifier
    {
        private static int counter;
        
        public readonly int id;
        public readonly StatModifierType type;
        public readonly float value;

        public StatModifier(StatModifierType type, float value)
        {
            this.id = counter++;
            this.type = type;
            this.value = value;
        }
    }

    public enum StatModifierType
    {
        Additive,
        Multiplicative,
    }
}