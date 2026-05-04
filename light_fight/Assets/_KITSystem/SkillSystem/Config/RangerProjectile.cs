using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _KITSystem.SkillSystem.Config
{
    public class RangerProjectile : BaseProjectile
    {
        [TitleGroup("Ranger : GameObject")] [Indent]
        public GameObject prefab;

        [Indent] public Vector3 offsetPivotPosition;

        [TitleGroup("Ranger : HitBox")] [GUIColor("GetButtonColor1"), OnValueChanged("ShapeTypeChanged"), Indent]
        public BaseShape.ShapeType shapeType;

        [FormerlySerializedAs("hitBox")] [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseShape shape;

        [TitleGroup("Ranger : Trajectory")]
        [GUIColor("GetButtonColor2"), OnValueChanged("TrajectoryTypeChanged"), Indent]
        public BaseTrajectory.TrajectoryType trajectoryType;

        [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseTrajectory trajectory;

        public RangerProjectile()
        {
            ShapeTypeChanged();
            TrajectoryTypeChanged();
        }

        private Color GetButtonColor1()
        {
            switch (shapeType)
            {
                case BaseShape.ShapeType.Square:
                    return Color.green;
                case BaseShape.ShapeType.Capsule:
                    return Color.red;
                case BaseShape.ShapeType.Circle:
                    return Color.yellow;
                case BaseShape.ShapeType.Cone:
                    return Color.blue;
                default:
                    return Color.white;
            }
        }

        private Color GetButtonColor2()
        {
            switch (trajectoryType)
            {
                case BaseTrajectory.TrajectoryType.Blend:
                    return Color.green;
                case BaseTrajectory.TrajectoryType.Stationary:
                    return Color.red;
                case BaseTrajectory.TrajectoryType.Boomerang:
                    return Color.yellow;
                case BaseTrajectory.TrajectoryType.Parabolic:
                    return Color.blue;
                case BaseTrajectory.TrajectoryType.Bullet:
                    return Color.cyan;
                case BaseTrajectory.TrajectoryType.Cannon:
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }

        private void ShapeTypeChanged()
        {
            if (shape == null || shape.Type != shapeType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseShape>();
                foreach (var type in types)
                {
                    BaseShape instance = Activator.CreateInstance(type) as BaseShape;
                    if (instance != null && instance.Type == shapeType)
                    {
                        shape = instance;
                        return;
                    }
                }
            }
        }

        private void TrajectoryTypeChanged()
        {
            if (trajectory == null || trajectory.Type != trajectoryType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseTrajectory>();
                foreach (var type in types)
                {
                    BaseTrajectory instance = Activator.CreateInstance(type) as BaseTrajectory;
                    if (instance != null && instance.Type == trajectoryType)
                    {
                        trajectory = instance;
                        return;
                    }
                }
            }
        }

        public override ProjectileType Type => ProjectileType.Ranger;
    }
}