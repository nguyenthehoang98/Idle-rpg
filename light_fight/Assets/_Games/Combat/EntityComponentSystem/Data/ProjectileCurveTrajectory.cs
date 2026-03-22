using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileCurveTrajectory : IComponentData
    {
        public BlobAssetReference<CurveBlob> Blob;
        public readonly float Duration;
        public readonly float MaxValue;

        public ProjectileCurveTrajectory(AnimationCurve curve, float maxValue, float duration)
        {
            Blob = CurveBlob.CreateCurveBlob(curve);
            MaxValue = maxValue;
            Duration = duration;
        } 
        
        public float Evaluate(float time)
        {
            return CurveBlob.Evaluate(ref Blob.Value, Mathf.Clamp(time / Duration, 0f, 0.9999f)) * MaxValue;
        }
    }
}