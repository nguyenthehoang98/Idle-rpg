using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CastProjectileConfig : BaseActionConfig
    {
        [HideLabel, TitleGroup("Damage")] public DamageTicket damageTicket = new DamageTicket();

        [TitleGroup("Collision"), Indent]
        [Tooltip("Giới hạn va chạm của viên đạn, nếu đủ số lần thì đạn sẽ tự hủy")]
        public int maximumCollision = 1;
        [Indent, Tooltip("Ngưỡng thời gian viên đạn có thể va chạm với 1 Object lần nữa. \n(Ví dụ bãi độc gây sát thương mỗi 0.3s nếu đứng trên nó)")]
        public float collisionResetIntervalInSeconds = 1 / 30f;
        
        [TitleGroup("GameObject")] 
        [Indent] public GameObject prefab;
        [Indent] public float2 offsetStartPosition;

        [TitleGroup("HitBox")] [GUIColor("GetButtonColor1"), OnValueChanged("ShapeTypeChanged"), Indent]
        public BaseShapeConfig.ShapeType shapeType;

        [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseShapeConfig shapeConfig;

        [TitleGroup("Trajectory")]
        [GUIColor("GetButtonColor2"), OnValueChanged("TrajectoryTypeChanged"), Indent]
        public BaseTrajectoryConfig.TrajectoryType trajectoryType;

        [SerializeReference, HideReferenceObjectPicker, HideLabel, Indent]
        public BaseTrajectoryConfig trajectoryConfig;
        
        
        public CastProjectileConfig()
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

        public override ActionType Type => ActionType.CastProjectile;

        public override float Duration
        {
            get
            {
                float max = 0;
                if (shapeConfig != null) max = Mathf.Max(max, shapeConfig.triggerTimeInSeconds);
                if (trajectoryConfig != null) max = Mathf.Max(max, trajectoryConfig.Duration);
                return max;
            }
        }
    }
    
    [Serializable]
    public class DamageTicket
    {
        [Indent] public bool isHpPercent = false;
        [Indent] public DamageTickerType damageTickerType = DamageTickerType.None;
        [HideIf("damageTickerType", DamageTickerType.None), Indent]
        public float damageTickerIntervalInSeconds = 1f;
    }
    
    public enum DamageTickerType
    {
        None,
        CasterInterval,
        TargetInterval
    }
}