using _KITSystem.SkillSystem.Config;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    public static class SkillFactory
    {
        private static SPU spu;
        private static IQuery query;

        public static void Initialize(SPU spu, IQuery query)
        {
            SkillFactory.spu = spu;
            SkillFactory.query = query;
        }

        public static int Build(float2 start, float2 goal, SkillFrameConfig skillFrameConfig)
        {
            return Build(spu, start, goal, skillFrameConfig);
        }

        public static int Build(SPU spu, SkillFrameConfig frameConfig)
        {
            float2 start = float2.zero;
            float2 goal = new float2(2, 0);
            return Build(spu, start, goal, frameConfig);
        }

        private static int Build(SPU spu, float2 start, float2 goal, SkillFrameConfig frameConfig)
        {
            int skillId = spu.GenerateSkillInstanceId();
            float lifeTimeInSeconds = frameConfig.defaultSkillConfig.lifeTimeInSeconds;
            foreach (EventConfig e in frameConfig.events)
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
                                new CastMeleeProjectileSkillAction(start, goal, shapes, spu,
                                    e.triggerConfig, lifeTimeInSeconds)
                            );
                        }
                        else if (cp.projectileType == BaseProjectileConfig.ProjectileType.Ranger)
                        {
                            var ranger = cp.projectileConfig as RangerProjectileConfig;
                            var trajectory = GetTrajectory(ranger.trajectoryConfig, start + ranger.offsetStartPosition, goal);
                            shapes = new BaseShapeAction[1] { GenerateShape(ranger.shapeConfig) };
                            spu.RequestAddAction(skillId,
                                new CastRangeProjectileSkillAction(trajectory, shapes, spu,
                                    e.triggerConfig, lifeTimeInSeconds)
                            );
                        }

                        break;
                    case BaseActionConfig.ActionType.TriggerEventId:
                        var triggerEventId = e.actionConfig as TriggerEventIdConfig;
                        spu.RequestAddAction(skillId,
                            new TriggerEventIdSkillAction(skillId, triggerEventId.eventId, 
                                spu, e.triggerConfig, lifeTimeInSeconds)
                        );
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
                    return new SquareShapeAction(square, query);
                case BaseShapeConfig.ShapeType.Circle:
                    var circle = shapeConfig as CircleShapeConfig;
                    return new CircleShapeAction(circle, query);
                default:
                    Debug.LogError($"Type {shapeConfig.Type} is not supported");
                    return null;
            }
        }

        static BaseTrajectoryAction GetTrajectory(BaseTrajectoryConfig trajectoryConfig, float2 start, float2 goal)
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
