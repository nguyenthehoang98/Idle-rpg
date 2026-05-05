using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [System.Serializable]
    public class BlendConstValue
    {
        [SerializeField] private BlendConstType type;
        [HideIf("type", BlendConstType.Blend)]
        [SerializeField] private float value;
        [ShowIf("type", BlendConstType.Curve)]
        [SerializeField] private AnimationCurve curve;

        [ShowIf("type", BlendConstType.Blend)]
        [SerializeField] private float fromValue;
        [ShowIf("type", BlendConstType.Blend)]
        [SerializeField] private float toValue;

        public BlendConstValue()
        {
        }
        
        public BlendConstValue(BlendConstType type)
        {
            this.type = type;
            this.value = 0;
            this.fromValue = 0;
            this.toValue = 1;
            this.curve = new AnimationCurve();
        }
        
        /// <param name="process">[0:1]</param>
        /// <returns></returns>
        public float Evaluate(float process)
        {
            switch (type)
            {
                case BlendConstType.Curve:
                    return curve.Evaluate(process) * value;
                case BlendConstType.Blend:
                    return Mathf.Lerp(fromValue, toValue, process);
                default:
                    return value;
            }
        }

        public enum BlendConstType
        {
            Constant,
            Curve,
            Blend,
        }
    }
}