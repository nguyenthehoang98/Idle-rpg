using System;
using LitMotion.Adapters;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace LitMotion.Animation.Components
{
    [Serializable]
    [LitMotionAnimationComponentMenu("Spline/Transform")]
    public sealed class TransformSplineComponentAnimation : LitMotionAnimationComponent
    {
        [SerializeField] SplineContainer spline;
        [SerializeField] Transform transform;
        [SerializeField] AnimationCurve curve;
        [SerializeField, Min(0)] float duration;

        public TransformSplineComponentAnimation()
        {
            type = "Spline";
        }

        public override float Duration() => duration;

        public override MotionHandle Play()
        {
            return LMotion.Create(0f, 1f, duration).Bind(t =>
            {
                spline.Evaluate(curve.Evaluate(t), out float3 localPos, out float3 tangent, out float3 upVector);
                Debug.DrawLine(transform.position, localPos, Color.red, 1f);
                transform.position = localPos;
                transform.rotation = Quaternion.LookRotation(tangent, upVector);
            });
        }
    }
}
