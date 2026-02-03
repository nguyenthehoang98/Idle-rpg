using AbilitySystem.Runtime.Data;

namespace AbilitySystem.Runtime.Abilities
{
    public sealed class TestAbilityAbility : BaseAbility<TestAbilityData>
    {
        public override void Initialize(TestAbilityData abilityData)
        {
            base.Initialize(abilityData);

            // TODO: TestAbilityAbility initialize logic here
        }

        public override void Execute()
        {
            base.Execute();

            // TODO: TestAbilityAbility execute logic here
        }

        public override void Cancel()
        {
            base.Cancel();

            // TODO: TestAbilityAbility cancel logic here
        }
    }
}