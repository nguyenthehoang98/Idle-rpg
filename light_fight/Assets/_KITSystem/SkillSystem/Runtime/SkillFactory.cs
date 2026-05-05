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
            int targetObjectId = 1;
            Vector3 start = Vector3.zero;
            Vector3 goal = new Vector3(2, 0, 0);
            float lifeTimeInSeconds = config.defaultSkillConfig.lifeTimeInSeconds;
            foreach (EventConfig e in config.events)
            {
                switch (e.actionConfig.Type)
                {
                    case BaseActionConfig.ActionType.CastProjectile:
                        var cp = e.actionConfig as CastProjectileConfig;
                        BaseShapeAction[] shapes;
                        if (cp.projectileType == BaseProjectileConfig.ProjectileType.Melee)
                        {
                            var melee = cp.projectileConfig as MeleeProjectileConfig;
                            shapes = new BaseShapeAction[melee.hitBoxes.Count];
                            for (int i = 0; i < melee.hitBoxes.Count; i++)
                                shapes[i] = GenerateShape(melee.hitBoxes[i]);
                            spu.RequestAddAction(skillId,
                                new CastMeleeProjectileAction(start, goal, shapes, e.triggerConfig, lifeTimeInSeconds)
                            );
                        }
                        else if (cp.projectileType == BaseProjectileConfig.ProjectileType.Ranger)
                        {
                            var ranger = cp.projectileConfig as RangerProjectileConfig;
                            if (ranger.trajectoryConfig.isRequireTargetToCast && targetObjectId == -1)
                                continue;
                            var trajectory = GetTrajectory(ranger.trajectoryConfig, start + ranger.offsetStartPosition, goal);
                            shapes = new BaseShapeAction[1] { GenerateShape(ranger.shapeConfig) };
                            spu.RequestAddAction(skillId,
                                new CastRangeProjectileAction(trajectory, shapes, e.triggerConfig, lifeTimeInSeconds)
                            );
                        }

                        break;
                }
            }
            
            return skillId;
        }

        static BaseShapeAction GenerateShape(BaseShapeConfig shapeConfig)
        {
            switch (shapeConfig.Type)
            {
                case BaseShapeConfig.ShapeType.Square:
                    var square = shapeConfig as SquareShapeConfig;
                    return new SquareShapeAction(square);
                case BaseShapeConfig.ShapeType.Circle:
                    var circle = shapeConfig as CircleShapeConfig;
                    return new CircleShapeAction(circle);
                default:
                    Debug.LogError($"Type {shapeConfig.Type} is not supported");
                    return null;
            }
        }

        static BaseTrajectoryAction GetTrajectory(BaseTrajectoryConfig trajectoryConfig, Vector3 start, Vector3 goal)
        {
            switch (trajectoryConfig.Type)
            {
                case BaseTrajectoryConfig.TrajectoryType.Stationary:
                    return new StationaryTrajectoryAction(start, goal);
                case BaseTrajectoryConfig.TrajectoryType.Bullet:
                    var bullet = trajectoryConfig as BulletTrajectoryConfig;
                    return new BulletTrajectoryAction(bullet.initialSpeed, bullet.acceleration, start, goal);
                case BaseTrajectoryConfig.TrajectoryType.Parabolic:
                    var parabolic = trajectoryConfig as ParabolicTrajectoryConfig;
                    return new ParabolicTrajectoryAction(parabolic.height, parabolic.distance, parabolic.duration, start, goal);
               case BaseTrajectoryConfig.TrajectoryType.Blend:
                   var blend = trajectoryConfig as BlendTrajectoryConfig;
                   return new BlendTrajectoryAction(blend.value, blend.duration, start, goal);
               case BaseTrajectoryConfig.TrajectoryType.Boomerang:
                   var boomerang = trajectoryConfig as BoomerangTrajectoryConfig;
                   return new BoomerangTrajectoryAction(boomerang.castPhase, boomerang.castDuration,
                       boomerang.returnPhase, boomerang.returnDuration, start, goal);
                default:
                    Debug.LogError($"Type {trajectoryConfig.Type} is not supported");
                    return null;
            }
        }
    }
}
