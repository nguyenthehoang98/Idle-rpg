using System;

namespace _GameToolkit.Skills
{
    public sealed class ModifierSkillAction : SkillAction
    {
        private readonly Action<ModifierSkillAction> onStartup;
        private readonly Action<ModifierSkillAction, float> onTickModifier;
        private readonly Action<ModifierSkillAction> onShutdown;

        public ModifierSkillAction(
            float lifeTime,
            Action<ModifierSkillAction> onStartup,
            Action<ModifierSkillAction, float> onTickModifier,
            Action<ModifierSkillAction> onShutdown) : base(lifeTime)
        {
            this.onStartup = onStartup;
            this.onTickModifier = onTickModifier;
            this.onShutdown = onShutdown;
        }

        public override void Startup()
        {
            base.Startup();
            onStartup?.Invoke(this);
        }

        protected override void OnTick(float deltaTime)
        {
            onTickModifier?.Invoke(this, deltaTime);
        }

        public override void Shutdown()
        {
            onShutdown?.Invoke(this);
            base.Shutdown();
        }
    }
}
