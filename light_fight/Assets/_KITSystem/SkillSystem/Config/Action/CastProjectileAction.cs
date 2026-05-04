using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config.Action
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
            [TitleGroup("Projectile : Damage"), Indent]
            public float damageScale = 1;
            [Indent]
            public bool isHpPercent = false;
            [Indent]
            public DamageTickerType damageTickerType = DamageTickerType.None;
            [HideIf("damageTickerType", DamageTickerType.None), Indent]
            public float damageTickerIntervalInSeconds = 1f;

            [TitleGroup("Projectile : Collision"), Indent]
            [Tooltip("Giới hạn va chạm của viên đạn, nếu đủ số lần thì đạn sẽ tự hủy")]
            public int maximumCollision = 1;
            [Tooltip("Ngưỡng thời gian viên đạn có thể va chạm với 1 Object lần nữa. \n(Ví dụ bãi độc gây sát thương mỗi 0.3s nếu đứng trên nó)")]
            public float collisionResetIntervalInSeconds = 1/30f;
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
                Square,
                Circle,
                Capsule,
                Cone
            }
        }

        [Serializable]
        public class SquareShape : BaseHitBox
        {
            [TitleGroup("", "Square")]
            public Vector2 size = new Vector2(1, 1);
            [TitleGroup("", "Square")]
            public Vector3 pivotRelativePosition;
            public override ShapeType Type => ShapeType.Square;
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