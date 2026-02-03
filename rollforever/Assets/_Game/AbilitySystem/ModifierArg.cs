using System;

namespace _Game.AbilitySystem
{
    [Serializable]
    public struct ModifierArg
    {
        public ModifierType type;
    }

    public enum ModifierType
    {
        None,
    }
}