using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileCurveTrajectory : IComponentData
    {
        private BlobAssetReference<CurveBlob> Blob;
        private readonly float Duration;
        private readonly float MaxValue;

        public ProjectileCurveTrajectory(BlobAssetReference<CurveBlob> blob, float maxValue, float duration)
        {
            Blob = blob;
            MaxValue = maxValue;
            Duration = duration;
        } 
        
        public float DistanceEvaluate(float time)
        {
            return CurveBlob.Evaluate(ref Blob.Value, Mathf.Clamp(time / Duration, 0f, 0.9999f)) * MaxValue;
        }
    }
}