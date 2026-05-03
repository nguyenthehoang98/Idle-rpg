using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Model
{
    [System.Serializable]
    public class BlendConstValue
    {
        public BlendConstType type;
        [HideIf("type", BlendConstType.Random)]
        public float value;
        [ShowIf("type", BlendConstType.Curve)]
        public AnimationCurve curve;

        [ShowIf("type", BlendConstType.Random)]
        public float fromValue;
        [ShowIf("type", BlendConstType.Random)]
        public float toValue;

        public enum BlendConstType
        {
            Constant,
            Curve,
            Random,
        }
    }
}