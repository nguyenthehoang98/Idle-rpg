using _Game.AbilitySystem;

namespace _Game.Battle.Data
{
    public struct UnitModifierData
    {
        public StatusEffect effect;

        public UnitModifierData(StatusEffect effect)
        {
            this.effect = effect;
        }
    }
}