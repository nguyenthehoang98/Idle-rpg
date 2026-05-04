using _KITSystem.SkillSystem.Config.Model;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastMeleeProjectileAction : CastProjectileAction
    {
        internal CastMeleeProjectileAction(BaseHitBoxAction[] hitBoxes, Trigger trigger, float lifeTime) : base(hitBoxes, trigger, lifeTime)
        {
        }
    }
}