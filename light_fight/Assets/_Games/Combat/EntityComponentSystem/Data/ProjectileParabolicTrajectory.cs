using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileParabolicTrajectory : IComponentData
    {
        public readonly float MaxHeight;
        private BlobAssetReference<CurveBlob> Blob;
        public readonly float Duration;

        public ProjectileParabolicTrajectory(AnimationCurve curve, float maxHeight, float duration)
        {
            Blob = CurveBlob.CreateCurveBlob(curve);
            Duration = duration;
            MaxHeight = maxHeight;
        } 
        
        public float HeightEvaluate(float value)
        {
            return CurveBlob.Evaluate(ref Blob.Value, value);
        }
    }
}