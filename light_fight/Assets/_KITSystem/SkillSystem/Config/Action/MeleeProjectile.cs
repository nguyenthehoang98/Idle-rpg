using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config.Action
{
    public partial class CastProjectileAction
    {
        [Serializable]
        public class MeleeProjectile : BaseProjectile
        {
            [TitleGroup("Melee")]
            [SerializeReference, HideReferenceObjectPicker] public List<BaseHitBox> hitBoxes = new List<BaseHitBox>();
            public override ProjectileType Type => ProjectileType.Melee;
        }
    }
}