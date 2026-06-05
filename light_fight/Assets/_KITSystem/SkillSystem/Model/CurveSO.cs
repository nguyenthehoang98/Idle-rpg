using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Model
{
    [CreateAssetMenu]
    public class CurveSO : ScriptableObject
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
    }
}