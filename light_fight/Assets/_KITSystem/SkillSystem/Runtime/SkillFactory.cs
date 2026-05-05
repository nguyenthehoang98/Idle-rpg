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
            Vector3 start = Vector3.zero;
            Vector3 goal = new Vector3(2, 0, 0);
            float lifeTimeInSeconds = config.defaultSkill.lifeTimeInSeconds;
            foreach (BaseEvent e in config.events)
            {
                switch (e.action.Type)
                {
                    case Config.BaseAction.ActionType.CastProjectile:
                        var cp = e.action as Config.CastProjectileAction;
                        BaseShapeAction[] shapes;
                        if (cp.projectileType == BaseProjectile.ProjectileType.Melee)
                        {
                            var melee = cp.projectile as MeleeProjectile;
                            shapes = new BaseShapeAction[melee.hitBoxes.Count];
                            for (int i = 0; i < melee.hitBoxes.Count; i++)
                                shapes[i] = GenerateShape(melee.hitBoxes[i]);
                            spu.RequestAddAction(skillId,
                                new CastMeleeProjectileAction(start, goal, shapes, e.trigger, lifeTimeInSeconds)
                            );
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

        static BaseTrajectoryAction GetTrajectory(BaseTrajectory trajectory, Vector3 start, Vector3 goal)
        {
            switch (trajectory.Type)
            {
                case BaseTrajectory.TrajectoryType.Stationary:
                    return new StationaryTrajectoryAction(start, goal);
                case BaseTrajectory.TrajectoryType.Bullet:
                    var bullet = trajectory as BulletTrajectory;
                    return new BulletTrajectoryAction(bullet.initialSpeed, bullet.acceleration, start, goal);
                default:
                    Debug.LogError($"Type {trajectory.Type} is not supported");
                    return null;
            }
        }
    }
}
