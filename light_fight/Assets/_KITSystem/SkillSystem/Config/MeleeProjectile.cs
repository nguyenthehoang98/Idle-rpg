using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class MeleeProjectile : BaseProjectile
    {
        [TitleGroup("Melee")]
        [SerializeReference, HideReferenceObjectPicker] public List<BaseShape> hitBoxes = new List<BaseShape>();
        public override ProjectileType Type => ProjectileType.Melee;
    }
}