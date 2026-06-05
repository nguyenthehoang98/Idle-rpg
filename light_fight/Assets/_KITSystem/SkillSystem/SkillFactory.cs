using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _KITSystem.Entity;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Model;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    public static class Skill
    {
        private static HashSet<string> cached = new HashSet<string>();

        public static async Task<int> Create(Spu spu, IQuery query, Func<int, bool> onDamageEntity,
            Vector2 startPosition, Vector2 targetPosition,
            string projectilePrefabPath, float lifeTime,
            DamageTickerType damageTickerType, float damageTickerInterval,
            int maxCollision, float resetCollisionInterval,
            ShapeType shapeType, float shapeTimerTrigger, float shapeDuration, Vector2 shapeOffsetRelative,
            float circleRadius, Vector2 squareSize,
            TrajectoryType trajectoryType, float bulletInitialSpeed, float bulletAcceleration,
            string boomerangCastingCurve, string boomerangReturningCurve, string iCurve,
            string parabolicHeightCurve, string parabolicDistanceCurve)
        {
            int instanceSkillId = spu.GenerateSkillInstanceId();

            GameObject go = await KitLoaded.LoadAsync<GameObject>(projectilePrefabPath);

            if (cached.Add(projectilePrefabPath))
            {
                KitPool.RegisterPool(go, true);
            }

            CurveSO boomerangCasting = trajectoryType == TrajectoryType.Boomerang
                ? await KitLoaded.LoadAsync<CurveSO>(boomerangCastingCurve)
                : null;
            CurveSO boomerangReturning = trajectoryType == TrajectoryType.Boomerang
                ? await KitLoaded.LoadAsync<CurveSO>(boomerangReturningCurve)
                : null;
            CurveSO curve = trajectoryType == TrajectoryType.Curve
                ? await KitLoaded.LoadAsync<CurveSO>(iCurve)
                : null;
            CurveSO parabolicHeight = trajectoryType == TrajectoryType.Parabolic
                ? await KitLoaded.LoadAsync<CurveSO>(parabolicHeightCurve)
                : null;
            CurveSO parabolicDistance = trajectoryType == TrajectoryType.Parabolic
                ? await KitLoaded.LoadAsync<CurveSO>(parabolicDistanceCurve)
                : null;

            BaseTrajectoryAction trajectoryAction = CreateTrajectory(trajectoryType, startPosition, targetPosition,
                bulletInitialSpeed, bulletAcceleration, boomerangCasting, boomerangReturning, curve, parabolicHeight,
                parabolicDistance
            );

            BaseShapeAction shapeAction = CreateShape(query, shapeType, shapeOffsetRelative, shapeTimerTrigger,
                shapeDuration, circleRadius, squareSize);
            
            GameObject projectile = KitPool.Instantiate(go);

            spu.RequestAddAction(instanceSkillId,
                new CastProjectileAction(spu, lifeTime, shapeAction, trajectoryAction, onDamageEntity, projectile,
                    damageTickerType, damageTickerInterval, maxCollision, resetCollisionInterval)
            );

            return instanceSkillId;
        }

        public static void Clear() => cached.Clear();
        
        private static BaseShapeAction CreateShape(IQuery query, ShapeType shapeType, Vector2 relativePosition, float timerTrigger, float duration,
            float radius, Vector2 squareSize)
        {
            switch (shapeType)
            {
                case ShapeType.Circle:
                    return new CircleShapeAction(query, relativePosition, timerTrigger, duration, radius);
                case ShapeType.Square:
                    return new SquareShapeAction(query, relativePosition, timerTrigger, duration, squareSize);
                default:
                    Debug.LogError($"Type {shapeType} is not supported");
                    return null;
            }
        }
        
        private  static BaseTrajectoryAction CreateTrajectory(TrajectoryType trajectoryType, Vector2 startPosition, Vector2 targetPosition,
            float bulletInitialSpeed, float bulletAcceleration,
            CurveSO boomerangCasting, CurveSO boomerangReturning, 
            CurveSO curve, 
            CurveSO parabolicHeight, CurveSO parabolicDistance)
        {
            switch (trajectoryType)
            {
                case TrajectoryType.Bullet:
                    return new BulletTrajectoryAction(bulletInitialSpeed, bulletAcceleration, startPosition, targetPosition);
                default:
                    Debug.LogError($"Type {trajectoryType} is not supported");
                    return null;
            }
        }
    }
}