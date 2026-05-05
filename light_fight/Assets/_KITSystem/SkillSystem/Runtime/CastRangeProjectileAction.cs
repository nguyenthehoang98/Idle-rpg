using _KITSystem.SkillSystem.Config;

namespace _KITSystem.SkillSystem.Runtime
{
    internal sealed class CastRangeProjectileAction : CastProjectileAction
    {
        internal CastRangeProjectileAction(BaseShapeAction[] shapes, TriggerConfig triggerConfig, float lifeTime) : base(shapes, triggerConfig, lifeTime)
        {
        }
    }
}