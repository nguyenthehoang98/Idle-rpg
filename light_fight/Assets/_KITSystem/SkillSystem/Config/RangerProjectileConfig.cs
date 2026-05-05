using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _KITSystem.SkillSystem.Config
{
    public class RangerProjectileConfig : BaseProjectileConfig
    {
        [TitleGroup("Ranger : GameObject")] [Indent]
        public GameObject prefab;

        [Indent] public Vector3 offsetPivotPosition;

        [TitleGroup("Ranger : HitBox")] [GUIColor("GetButtonColor1"), OnValueChanged("ShapeTypeChanged"), Indent]
        public BaseShapeConfig.ShapeType shapeType;

        [FormerlySerializedAs("hitBox")] [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseShapeConfig shapeConfig;

        [TitleGroup("Ranger : Trajectory")]
        [GUIColor("GetButtonColor2"), OnValueChanged("TrajectoryTypeChanged"), Indent]
        public BaseTrajectoryConfig.TrajectoryType trajectoryType;

        [FormerlySerializedAs("trajectory")] [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseTrajectoryConfig trajectoryConfig;

        public RangerProjectileConfig()
        {
            ShapeTypeChanged();
            TrajectoryTypeChanged();
        }

        private Color GetButtonColor1()
        {
            switch (shapeType)
            {
                case BaseShapeConfig.ShapeType.Square:
                    return Color.green;
                case BaseShapeConfig.ShapeType.Capsule:
                    return Color.red;
                case BaseShapeConfig.ShapeType.Circle:
                    return Color.yellow;
                case BaseShapeConfig.ShapeType.Cone:
                    return Color.blue;
                default:
                    return Color.white;
            }
        }

        private Color GetButtonColor2()
        {
            switch (trajectoryType)
            {
                case BaseTrajectoryConfig.TrajectoryType.Blend:
                    return Color.green;
                case BaseTrajectoryConfig.TrajectoryType.Stationary:
                    return Color.red;
                case BaseTrajectoryConfig.TrajectoryType.Boomerang:
                    return Color.yellow;
                case BaseTrajectoryConfig.TrajectoryType.Parabolic:
                    return Color.blue;
                case BaseTrajectoryConfig.TrajectoryType.Bullet:
                    return Color.cyan;
                default:
                    return Color.white;
            }
        }

        private void ShapeTypeChanged()
        {
            if (shapeConfig == null || shapeConfig.Type != shapeType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseShapeConfig>();
                foreach (var type in types)
                {
                    BaseShapeConfig instance = Activator.CreateInstance(type) as BaseShapeConfig;
                    if (instance != null && instance.Type == shapeType)
                    {
                        shapeConfig = instance;
                        return;
                    }
                }
            }
        }

        private void TrajectoryTypeChanged()
        {
            if (trajectoryConfig == null || trajectoryConfig.Type != trajectoryType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseTrajectoryConfig>();
                foreach (var type in types)
                {
                    BaseTrajectoryConfig instance = Activator.CreateInstance(type) as BaseTrajectoryConfig;
                    if (instance != null && instance.Type == trajectoryType)
                    {
                        trajectoryConfig = instance;
                        return;
                    }
                }
            }
        }

        public override ProjectileType Type => ProjectileType.Ranger;
    }
}