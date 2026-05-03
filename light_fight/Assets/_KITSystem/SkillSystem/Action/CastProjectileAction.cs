using System;
using _KITSystem.SkillSystem.Trigger;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Action
{
    [Serializable]
    public partial class CastProjectileAction : BaseAction
    {
        [OnValueChanged("ProjectileTypeChanged")]
        public BaseProjectile.ProjectileType projectileType;
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseProjectile projectile;

        public override ActionType Type => ActionType.CastProjectile;

        public CastProjectileAction()
        {
            ProjectileTypeChanged();
        }
        
        private void ProjectileTypeChanged()
        {
            if (projectile == null || projectile.Type != projectileType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseProjectile>();
                foreach (var type in types)
                {
                    BaseProjectile instance = Activator.CreateInstance(type) as BaseProjectile;
                    if (instance != null && instance.Type == projectileType)
                    {
                        projectile = instance;
                        return;
                    }
                }
            }
        }
    }

    public partial class CastProjectileAction
    {
        [Serializable]
        public abstract class BaseProjectile
        {
            [TitleGroup("Default")]
            public float damageScale = 1;
            public bool isHpPercent = false;
            public DamageTickerType damageTickerType = DamageTickerType.None;
            [HideIf("damageTickerType", DamageTickerType.None)]
            public float damageTickerIntervalInSeconds = 1f;
            public abstract ProjectileType Type { get; }
            
            public enum ProjectileType
            {
                Melee, Ranger
            }
        }

        public enum DamageTickerType
        {
            None,
            CasterInterval,
            TargetInterval
        }

        [Serializable]
        public abstract class BaseHitBox
        {
            public float triggerTimeInSeconds;
            public abstract ShapeType Type { get; }

            public enum ShapeType
            {
                Box,
                Circle,
                Capsule,
                Cone
            }
        }

        [Serializable]
        public class BoxShape : BaseHitBox
        {
            public Vector3 size = new Vector3(1, 1, 1);
            public Vector3 pivotRelativePosition;
            public override ShapeType Type => ShapeType.Box;
            public PivotType pivotType;

            public enum PivotType
            {
                Center,
                BottomLeft,
                BottomRight,
                TopLeft,
                TopRight,
            }
        }

        [Serializable]
        public class CircleShape : BaseHitBox
        {
            public float radius = 1f;
            public Vector3 relativePositionOfCenter;
            public override ShapeType Type => ShapeType.Circle;
        }

        [Serializable]
        public class CapsuleShape : BaseHitBox
        {
            public Vector3 relativePositionOfCenter;
            public float radius = 1;
            public float height = 2;
            public override ShapeType Type => ShapeType.Capsule;
        }

        [Serializable]
        public class ConeShape : BaseHitBox
        {
            public Vector3 relativePos;
            public float widthTop;
            public float widthBottom;
            public float height;
            public int n;
            public override ShapeType Type => ShapeType.Cone;
        }
    }
}