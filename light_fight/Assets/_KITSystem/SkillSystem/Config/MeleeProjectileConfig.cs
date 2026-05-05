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

        public override float Duration
        {
            get
            {
                float max = 0;
                foreach (var shape in hitBoxes)
                {
                    max = Mathf.Max(max, shape.triggerTimeInSeconds);
                }

                return max;
            }
        }
    }
}