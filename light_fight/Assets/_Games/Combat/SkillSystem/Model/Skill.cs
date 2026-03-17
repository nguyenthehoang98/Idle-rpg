using _Games.Combat.SkillSystem.Config;

namespace _Games.Combat.SkillSystem.Model
{
    public sealed class Skill
    {
        public readonly SkillMainModule main;
        public readonly ProjectileSO projectile;
        public readonly BaseColliderSO collider;
        public readonly BaseTrajectorySO trajectory;
        public readonly BaseModifierSO[] modifiers;
        public readonly BaseBehaviorSO[] behaviors;

        public Skill(SkillMainModule main, ProjectileSO projectile, BaseColliderSO collider, BaseTrajectorySO trajectory, BaseModifierSO[] modifiers, BaseBehaviorSO[] behaviors)
        {
            this.projectile = projectile;
            this.collider = collider;
            this.trajectory = trajectory;
            this.modifiers = modifiers;
            this.behaviors = behaviors;
            this.main = main;
        }
    }
}