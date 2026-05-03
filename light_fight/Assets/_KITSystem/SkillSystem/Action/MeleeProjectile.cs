using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Action
{
    public partial class CastProjectileAction
    {
        [Serializable]
        public class MeleeProjectile : BaseProjectile
        {
            [TitleGroup("Melee")]
            public AnchorType anchor;
            [SerializeReference, HideReferenceObjectPicker] public List<BaseHitBox> hitBoxes = new List<BaseHitBox>();
            public override ProjectileType Type => ProjectileType.Melee;
        }

        public enum AnchorType
        {
            None,
            Caster,
            Target
        }
    }
}