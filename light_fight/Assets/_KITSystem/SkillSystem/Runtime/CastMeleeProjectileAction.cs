using _KITSystem.SkillSystem.Config.Model;

namespace _KITSystem.SkillSystem.Runtime
{
    public sealed class CastMeleeProjectileAction : BaseAction
    {
        public CastMeleeProjectileAction(Trigger.TriggerType type, int eventId, float timer, bool isMultiplierTrigger, float lifeTime) : base(type, eventId, timer, isMultiplierTrigger, lifeTime)
        {
        }
    }
}