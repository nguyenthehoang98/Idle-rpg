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
        public float time, timeNor, height, heightNor;
        public float3 position;

        public ProjectileParabolicTrajectory(AnimationCurve curve, float maxHeight, float duration)
        {
            Blob = CurveBlob.CreateCurveBlob(curve);
            Duration = duration;
            MaxHeight = maxHeight;
            time = height = timeNor = heightNor = 0;
            position = float3.zero;
        } 
        
        public float HeightEvaluate(float value)
        {
            return CurveBlob.Evaluate(ref Blob.Value, value);
        }
    }
}