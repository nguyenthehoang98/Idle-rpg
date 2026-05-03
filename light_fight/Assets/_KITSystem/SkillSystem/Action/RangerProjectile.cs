using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Action
{
    public partial class CastProjectileAction
    {
        public partial class RangerProjectile
        {
            [TitleGroup("Ranger"), Required]
            public GameObject prefab;
            public Vector3 offsetPivotPosition;
            
            [TitleGroup("Hitbox")]
            [GUIColor("GetButtonColor"), OnValueChanged("ShapeTypeChanged")]
            public BaseHitBox.ShapeType shapeType;
            [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseHitBox hitBox;
            
            [TitleGroup("Trajectory")]
            [GUIColor("GetButtonColor"), OnValueChanged("TrajectoryTypeChanged")]
            public BaseTrajectory.TrajectoryType trajectoryType; 
            [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseTrajectory trajectory;

            public RangerProjectile()
            {
                ShapeTypeChanged();
                TrajectoryTypeChanged();
            }
            
            private Color GetButtonColor()
            {
                switch (shapeType)
                {
                    case BaseHitBox.ShapeType.Box:
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
                public DirectionRepresentationType type;
                [HideIf("type", DirectionRepresentationType.Vector)]
                public Vector3 direction;
                [HideIf("type", DirectionRepresentationType.Angle)]
                public float angle;
                    
                public enum DirectionRepresentationType
                {
                    Vector,
                    Angle
                }
                
                public abstract TrajectoryType Type { get; }

                public enum TrajectoryType
                {
                    Stationary,
                    Bullet,
                    Cannon,
                    Arrow,
                    Boomerang,
                    Ball,
                    JointFollower
                }
            }
        }
    }
}