using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileParabolicTrajectory : IComponentData
    {
        public readonly float MaxHeight;
        public readonly float Duration;
        private readonly BlobAssetReference<CurveBlob> Blob;

        public ProjectileParabolicTrajectory(BlobAssetReference<CurveBlob> blob, float maxHeight, float duration)
        {
            Blob = blob;
            Duration = duration;
            MaxHeight = maxHeight;
        } 
        
        public float HeightEvaluate(float value)
        {
            return CurveBlob.Evaluate(ref Blob.Value, value);
        }
    }
}