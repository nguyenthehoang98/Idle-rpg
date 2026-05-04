using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Config.Action;

namespace _KITSystem.SkillSystem.Runtime
{
    public static class SkillFactory
    {
        public static int Build(SPU spu, SkillConfig config)
        {
            int skillId = spu.GenerateSkillInstanceId();
            int targetObjectId = -1;
            float lifeTimeInSeconds = config.defineSkill.lifeTimeInSeconds;
            foreach (BaseEvent e in config.events)
            {
                switch (e.action.Type)
                {
                    case Config.Action.BaseAction.ActionType.CastProjectile:
                        var cp = e.action as CastProjectileAction;
                        if (cp.projectileType == CastProjectileAction.BaseProjectile.ProjectileType.Melee)
                        {
                            spu.RequestAddAction(skillId, new CastMeleeProjectileAction(
                                e.trigger.type, e.trigger.eventId, e.trigger.timer,
                                e.trigger.isMultiplierTrigger, lifeTimeInSeconds)
                            );
                        }
                        else if(cp.projectileType == CastProjectileAction.BaseProjectile.ProjectileType.Ranger)
                        {
                        }
                        break;
                }
            }
            
            return skillId;
        }
    }
}
