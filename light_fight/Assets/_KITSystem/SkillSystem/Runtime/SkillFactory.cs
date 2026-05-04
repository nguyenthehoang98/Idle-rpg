using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Config.Action;
using UnityEngine;

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
                        var cp = e.action as Config.Action.CastProjectileAction;
                        if (cp.projectileType == Config.Action.CastProjectileAction.BaseProjectile.ProjectileType.Melee)
                        {
                            /*spu.RequestAddAction(skillId, new CastMeleeProjectileAction(
                                e.trigger.type, e.trigger.eventId, e.trigger.timer,
                                e.trigger.isMultiplierTrigger, lifeTimeInSeconds)
                            );*/
                        }
                        else if(cp.projectileType == Config.Action.CastProjectileAction.BaseProjectile.ProjectileType.Ranger)
                        {
                        }
                        break;
                }
            }
            
            return skillId;
        }

        static CastProjectileAction.BaseHitBoxAction GenerateHitBox(
            Config.Action.CastProjectileAction.BaseHitBox hitBox)
        {
            switch (hitBox.Type)
            {
                case Config.Action.CastProjectileAction.BaseHitBox.ShapeType.Square:
                    var square = hitBox as Config.Action.CastProjectileAction.SquareShape;
                    return new CastProjectileAction.SquareHitBoxAction(square);
                case Config.Action.CastProjectileAction.BaseHitBox.ShapeType.Circle:
                    var circle = hitBox as Config.Action.CastProjectileAction.CircleShape;
                    return new CastProjectileAction.CircleHitBoxAction(circle);
                default:
                    Debug.LogError($"Type {hitBox.Type} is not supported");
                    return null;
            }
        }
    }
}
