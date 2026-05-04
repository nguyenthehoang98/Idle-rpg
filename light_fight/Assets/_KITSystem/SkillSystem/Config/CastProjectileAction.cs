using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public partial class CastProjectileAction : BaseAction
    {
        [GUIColor("GetButtonColor"), OnValueChanged("ProjectileTypeChanged"), HideLabel, BoxGroup]
        public BaseProjectile.ProjectileType projectileType;

        [SerializeReference, HideReferenceObjectPicker, HideLabel]
        public BaseProjectile projectile;

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

    public enum DamageTickerType
    {
        None,
        CasterInterval,
        TargetInterval
    }
}