using _KITSystem.SkillSystem.Config;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract partial class CastProjectileAction : BaseAction
    {
        private BaseShapeAction[] shapes;
        
        protected internal CastProjectileAction(BaseShapeAction[] shapes, Trigger trigger, float lifeTime) : base(trigger, lifeTime)
        {
            this.shapes = shapes;
        }
    }
}