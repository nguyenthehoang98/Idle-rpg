using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileCurveTrajectory : IComponentData
    {
        private BlobAssetReference<CurveBlob> Blob;
        private readonly float Duration;
        private readonly float Speed;

        public ProjectileCurveTrajectory(BlobAssetReference<CurveBlob> blob, float speed, float duration)
        {
            Blob = blob;
            Speed = speed;
            Duration = duration;
        } 
        
        public float DistanceEvaluate(float time)
        {
            return CurveBlob.Evaluate(ref Blob.Value, Mathf.Clamp(time / Duration, 0f, 0.9999f)) * Speed;
        }
    }
}