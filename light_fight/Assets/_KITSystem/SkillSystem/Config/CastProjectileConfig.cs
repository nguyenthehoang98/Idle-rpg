using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class CastProjectileConfig : BaseActionConfig
    {
        [GUIColor("GetButtonColor"), OnValueChanged("ProjectileTypeChanged"), HideLabel, BoxGroup]
        public BaseProjectileConfig.ProjectileType projectileType;

        [FormerlySerializedAs("projectile")] [SerializeReference, HideReferenceObjectPicker, HideLabel]
        public BaseProjectileConfig projectileConfig;

        public override ActionType Type => ActionType.CastProjectile;

        public CastProjectileConfig()
        {
            ProjectileTypeChanged();
        }

        private Color GetButtonColor()
        {
            switch (projectileType)
            {
                case BaseProjectileConfig.ProjectileType.Melee:
                    return new Color(0, 1, 1);
                case BaseProjectileConfig.ProjectileType.Ranger:
                    return new Color(1, 1, 0);
                default:
                    return Color.white;
            }
        }

        private void ProjectileTypeChanged()
        {
            if (projectileConfig == null || projectileConfig.Type != projectileType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseProjectileConfig>();
                foreach (var type in types)
                {
                    BaseProjectileConfig instance = Activator.CreateInstance(type) as BaseProjectileConfig;
                    if (instance != null && instance.Type == projectileType)
                    {
                        projectileConfig = instance;
                        return;
                    }
                }
            }
        }
    }

    public enum DamageTickerType
    {
        None,
        CasterInterval,
        TargetInterval
    }
}