using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class MeleeProjectileConfig : BaseProjectileConfig
    {
        [TitleGroup("Melee")]
        [SerializeReference, HideReferenceObjectPicker] public List<BaseShapeConfig> hitBoxes = new List<BaseShapeConfig>();
        public override ProjectileType Type => ProjectileType.Melee;
    }
}