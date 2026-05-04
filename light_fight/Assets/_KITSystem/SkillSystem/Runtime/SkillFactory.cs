using _KITSystem.SkillSystem.Config;
using UnityEngine;
using SkillConfig = _KITSystem.SkillSystem.Config.SkillConfig;

namespace _KITSystem.SkillSystem.Runtime
{
    public static class SkillFactory
    {
        public static int Build(SPU spu, SkillConfig config)
        {
            int skillId = spu.GenerateSkillInstanceId();
            int targetObjectId = -1;
            float lifeTimeInSeconds = config.defaultSkill.lifeTimeInSeconds;
            foreach (BaseEvent e in config.events)
            {
                switch (e.action.Type)
                {
                    case Config.BaseAction.ActionType.CastProjectile:
                        var cp = e.action as Config.CastProjectileAction;
                        if (cp.projectileType == BaseProjectile.ProjectileType.Melee)
                        {
                            /*spu.RequestAddAction(skillId, new CastMeleeProjectileAction(
                                e.trigger.type, e.trigger.eventId, e.trigger.timer,
                                e.trigger.isMultiplierTrigger, lifeTimeInSeconds)
                            );*/
                        }
                        else if(cp.projectileType == BaseProjectile.ProjectileType.Ranger)
                        {
                        }
                        break;
                }
            }
            
            return skillId;
        }

        static BaseShapeAction GenerateShape(BaseShape shape)
        {
            switch (shape.Type)
            {
                case BaseShape.ShapeType.Square:
                    var square = shape as SquareShape;
                    return new SquareShapeAction(square);
                case BaseShape.ShapeType.Circle:
                    var circle = shape as CircleShape;
                    return new CircleShapeAction(circle);
                default:
                    Debug.LogError($"Type {shape.Type} is not supported");
                    return null;
            }
        }
    }
}
