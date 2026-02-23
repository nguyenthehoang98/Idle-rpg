using System;

namespace _Game.Battle.AbilitySystem
{
    [Serializable]
    public struct StateModifierArg
    {
        public StateModifierType type;
        public ModifierGroup group;
        public float value;
        public float duration;
    }

    public enum StateModifierType
    {
        None,
        Knockback,
        Stun,
    }

    public enum ModifierGroup
    {
        Target,
        Self, 
        Teammate,
    }
}