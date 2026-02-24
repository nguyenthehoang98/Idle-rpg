using _Game.Battle.AbilitySystem;

namespace _Game.Battle.Ecs.Data
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