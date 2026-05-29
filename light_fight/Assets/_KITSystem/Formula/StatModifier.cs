using System;

namespace _KITSystem.Formula
{
    [Serializable]
    public readonly struct StatModifier
    {
        public readonly StatType StatType;
        public readonly StatModifierType ModifierType;
        public readonly float Value;
        public readonly int Order;
        public readonly object Source;

        public StatModifier(StatType statType, StatModifierType modifierType, float value, object source = null, int order = 0)
        {
            StatType = statType;
            ModifierType = modifierType;
            Value = value;
            Source = source;
            Order = order;
        }
    }
}