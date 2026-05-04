using _KITSystem.SkillSystem.Config;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastMeleeProjectileAction : CastProjectileAction
    {
        internal CastMeleeProjectileAction(BaseShapeAction[] shapes, Trigger trigger, float lifeTime) : base(shapes, trigger, lifeTime)
        {
        }
    }
}