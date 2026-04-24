using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileBoomerangTrajectory : IComponentData
    {
        public bool IsCastingPhase;
        private BlobAssetReference<CurveBlob> CastBlob;
        private readonly float CastSpeed;
        private BlobAssetReference<CurveBlob> ReturnBlob;
        private readonly float ReturnSpeed;
        public readonly float Duration;

        public ProjectileBoomerangTrajectory(BlobAssetReference<CurveBlob> castBlob, float castSpeed, 
            BlobAssetReference<CurveBlob> returnBlob, float returnSpeed, 
            float duration)
        {
            CastBlob = castBlob;
            CastSpeed = castSpeed;
            ReturnBlob = returnBlob;
            ReturnSpeed = returnSpeed;
            Duration = duration;
            IsCastingPhase = true;
        }
        
        public float CastDistanceEvaluate(float time)
        {
            return CurveBlob.Evaluate(ref CastBlob.Value, Mathf.Clamp(time / Duration, 0f, 0.9999f)) * CastSpeed;
        }
        
        public float ReturnDistanceEvaluate(float time)
        {
            return CurveBlob.Evaluate(ref ReturnBlob.Value, Mathf.Clamp(time / Duration, 0f, 0.9999f)) * ReturnSpeed;
        }
    }
}