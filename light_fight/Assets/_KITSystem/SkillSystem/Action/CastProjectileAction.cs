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
        [GUIColor("GetButtonColor"), OnValueChanged("ProjectileTypeChanged"), HideLabel, BoxGroup]
        public BaseProjectile.ProjectileType projectileType;
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseProjectile projectile;

        public override ActionType Type => ActionType.CastProjectile;

        public CastProjectileAction()
        {
            ProjectileTypeChanged();
        }

        private Color GetButtonColor()
        {
            switch (projectileType)
            {
                case BaseProjectile.ProjectileType.Melee:
                    return new Color(0, 1, 1);
                case BaseProjectile.ProjectileType.Ranger:
                    return new Color(1, 1, 0);
                default:
                    return Color.white;
            }
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
            [TitleGroup("Projectile : Damage")]
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
            [TitleGroup("", "Box")]
            public Vector3 size = new Vector3(1, 1, 1);
            [TitleGroup("", "Box")]
            public Vector3 pivotRelativePosition;
            public override ShapeType Type => ShapeType.Box;
            [TitleGroup("", "Box")]
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
            [TitleGroup("Circle")]
            public float radius = 1f;
            [TitleGroup("Circle")]
            public Vector3 relativePositionOfCenter;
            public override ShapeType Type => ShapeType.Circle;
        }

        [Serializable]
        public class CapsuleShape : BaseHitBox
        {
            [TitleGroup("Capsule")]
            public Vector3 relativePositionOfCenter;
            [TitleGroup("Capsule")]
            public float radius = 1;
            [TitleGroup("Capsule")]
            public float height = 2;
            public override ShapeType Type => ShapeType.Capsule;
        }

        [Serializable]
        public class ConeShape : BaseHitBox
        {
            [TitleGroup("Cone")]
            public Vector3 relativePos;
            [TitleGroup("Cone")]
            public float widthTop;
            [TitleGroup("Cone")]
            public float widthBottom;
            [TitleGroup("Cone")]
            public float height;
            [TitleGroup("Cone")]
            public int n;
            public override ShapeType Type => ShapeType.Cone;
        }
    }
}