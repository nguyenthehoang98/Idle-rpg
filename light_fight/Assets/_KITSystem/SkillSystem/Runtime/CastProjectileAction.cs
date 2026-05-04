using _KITSystem.SkillSystem.Config.Model;

namespace _KITSystem.SkillSystem.Runtime
{
    internal abstract partial class CastProjectileAction : BaseAction
    {
        private BaseHitBoxAction[] hitBoxes;
        
        protected internal CastProjectileAction(BaseHitBoxAction[] hitBoxes, Trigger trigger, float lifeTime) : base(trigger, lifeTime)
        {
            this.hitBoxes = hitBoxes;
        }
    }
}