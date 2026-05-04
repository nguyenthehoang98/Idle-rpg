using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [TitleGroup("Ranger : GameObject")]
            [Indent]
            public GameObject prefab;
            [Indent]
            public Vector3 offsetPivotPosition;
            
            [TitleGroup("Ranger : HitBox")]
            [GUIColor("GetButtonColor1"), OnValueChanged("ShapeTypeChanged"), Indent]
            public BaseHitBox.ShapeType shapeType;
            [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
            public BaseHitBox hitBox;
            
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
                    case BaseHitBox.ShapeType.Square:
                        return Color.green;
                    case BaseHitBox.ShapeType.Capsule:
                        return Color.red;
                    case BaseHitBox.ShapeType.Circle:
                        return Color.yellow;
                    case BaseHitBox.ShapeType.Cone:
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
                if (hitBox == null || hitBox.Type != shapeType)
                {
                    Type[] types = TypeUtils.GetAllTypeThatImplement<BaseHitBox>();
                    foreach (var type in types)
                    {
                        BaseHitBox instance = Activator.CreateInstance(type) as BaseHitBox;
                        if (instance != null && instance.Type == shapeType)
                        {
                            hitBox = instance;
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
        
        public partial class RangerProjectile : BaseProjectile
        {
            [Serializable]
            public abstract class BaseTrajectory
            {
                public bool isRequireTargetToCast;
                public abstract TrajectoryType Type { get; }

                public enum TrajectoryType
                {
                    Stationary,
                    Bullet,
                    Cannon,
                    Boomerang,
                    Blend,
                    Parabolic
                }
            }
        }
    }
}